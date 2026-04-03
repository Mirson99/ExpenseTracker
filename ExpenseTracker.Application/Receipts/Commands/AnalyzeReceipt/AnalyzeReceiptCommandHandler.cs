using System.Text.Json;
using Amazon.S3;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Polly;
using Polly.Registry;

namespace ExpenseTracker.Application.Receipts.Commands.AnalyzeReceipt;

public class AnalyzeReceiptCommandHandler(
    IAmazonS3 s3Client,
    IChatClient chatClient,
    ResiliencePipelineProvider<string> pipelineProvider, // <-- ZMIANA: Wstrzykujemy Providera
    IAppDbContext dbContext,
    INotificationService notificationService ) : IRequestHandler<AnalyzeReceiptCommand, bool>
{
    public async Task<bool> Handle(AnalyzeReceiptCommand request, CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses.FindAsync([request.ExpenseId], cancellationToken);

        if (expense == null || expense.Status != ExpenseStatus.Processing)
        {
            return false;
        }

        try
        {
            // 1. Pobranie pliku z MinIO
            using var getObjectResponse = await s3Client.GetObjectAsync("receipts-bucket", request.StorageKey, cancellationToken);
            using var memoryStream = new MemoryStream();
            await getObjectResponse.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();

            // 2. Pobranie odpowiedniego potoku Polly po nazwie
            var resiliencePipeline = pipelineProvider.GetPipeline("gemini-pipeline"); // <-- ZMIANA: Pobranie potoku

            var availableCategories = await dbContext.Categories
                .Select(c => new {c.Id, c.Name}).ToListAsync(cancellationToken);

            // Łączymy je w jeden string, żeby wstrzyknąć do promptu
            var categoriesString = string.Join(", ", availableCategories.Select(c => c.Name).ToList());

            // 2. Profesjonalny System Prompt po angielsku
            var systemPrompt = $@"You are an expert accounting assistant.
                Your task is to analyze the provided receipt image and extract the necessary financial data.
                Strict processing rules:
                1. Extract 'MerchantName', 'TotalAmount', and 'Date' accurately.
                2. Generate a concise 'Description' (max 2 sentences) explaining what was purchased based on the items on the receipt.
                3. Classify the expense into EXACTLY ONE of these available categories: [{categoriesString}]. 
                4. If the exact category is not obvious, choose the closest logical match from the provided list. DO NOT invent new categories.
                5. Return ONLY a valid JSON object matching the requested schema.";

            var chatMessages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, [
                    new TextContent("Analyze this receipt and extract the data:"),
                    new DataContent(imageBytes, "image/jpeg") 
                ])
            };

            // 3. Najlepsze praktyki: Wymuszamy na modelu odpowiedź w czystym JSON (bez obudowy markdown)
            var chatOptions = new ChatOptions 
            { 
                ResponseFormat = ChatResponseFormat.Json 
            };

            // 4. Wywołanie LLM zabezpieczone przez Polly (przekazujemy chatOptions)
            var response = await resiliencePipeline.ExecuteAsync(
                async ct => await chatClient.GetResponseAsync(chatMessages, chatOptions, cancellationToken: ct),
                cancellationToken);

            var extractedData = JsonSerializer.Deserialize<ExtractedReceiptData>(response.Text);
            
            if (extractedData != null)
            {
                var matchedCategory = availableCategories.FirstOrDefault(
                    c => c.Name.Equals(extractedData.Category, StringComparison.OrdinalIgnoreCase));

                var finalCategoryId = matchedCategory?.Id ?? 12; // Lub inne domyślne zachowanie biznesowe
                // Wywołanie metody domenowej z Rich Domain Model
                expense.ApplyAiRecognition(
                    extractedData.TotalAmount, 
                    "PLN",
                    extractedData.Date,
                    extractedData.MerchantName,
                    extractedData.Description,
                    finalCategoryId);
                
                var notificationMessage = $"Paragon ze sklepu {extractedData.MerchantName} został przetworzony i czeka na weryfikację.";

                await notificationService.SendToUserAsync(
                    expense.UserId.ToString(), 
                    notificationMessage, 
                    cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            expense.MarkAsFailed();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw; 
        }
    }
}
public record ExtractedReceiptData(
    string MerchantName, 
    decimal TotalAmount, 
    DateTime Date, 
    string Description, 
    string Category);