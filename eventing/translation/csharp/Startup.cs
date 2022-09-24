// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace translation
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            logger.LogInformation("Service is starting...");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/", async context =>
                {
                    var cloudEvent = await context.Request.ReadCloudEventAsync();

                    logger.LogInformation("Received CloudEvent\n" + GetEventLog(cloudEvent));

                    try
                    {
                        var jObject = (JObject)JToken.Parse((string)cloudEvent.Data);
                        var data = jObject["message"]["data"];
                        var decodedData = GetDecodedData((string)data);
                        logger.LogInformation($"Decoded data: {decodedData}");

                        var translationRequest = JsonConvert.DeserializeObject<TranslationRequest>(decodedData);
                        logger.LogInformation($"Calling Translation API with request: {translationRequest}");

                        var response = await TranslateText(translationRequest);
                        logger.LogInformation($"Translated text: {response.TranslatedText}");
                        if (response.DetectedSourceLanguage != null)
                        {
                            logger.LogInformation($"Detected language: {response.DetectedSourceLanguage}");
                        }
                        await context.Response.WriteAsync(response.TranslatedText);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Something went wrong: " + e.Message);
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync(e.Message);
                    }
                });
            });
        }


        public string GetDecodedData(string data) => (
            string.IsNullOrEmpty(data) ?
                string.Empty :
                Encoding.UTF8.GetString(Convert.FromBase64String(data)));

        private async Task<TranslationResult> TranslateText(TranslationRequest translationRequest)
        {
            ValidateTranslationRequest(translationRequest);

            var client = TranslationClient.Create();
            var response = await client.TranslateTextAsync(translationRequest.Text, translationRequest.To, translationRequest.From);
            return response;
        }

        private void ValidateTranslationRequest(TranslationRequest translationRequest)
        {
            if (translationRequest == null)
            {
                throw new ArgumentException("Translation request cannot be null");
            }

            if (string.IsNullOrEmpty(translationRequest.Text))
            {
                throw new ArgumentException("Translation text cannot be empty or null");
            }

            if (string.IsNullOrEmpty(translationRequest.To))
            {
                throw new ArgumentException("Translation 'to' cannot be empty or null");
            }
        }

        private string GetEventLog(CloudEvent cloudEvent)
        {
            return $"ID: {cloudEvent.Id}\n"
                + $"Source: {cloudEvent.Source}\n"
                + $"Type: {cloudEvent.Type}\n"
                + $"Subject: {cloudEvent.Subject}\n"
                + $"DataSchema: {cloudEvent.DataSchema}\n"
                + $"DataContentType: {cloudEvent.DataContentType}\n"
                + $"Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}\n"
                + $"SpecVersion: {cloudEvent.SpecVersion}\n"
                + $"Data: {cloudEvent.Data}";
        }
    }
}
