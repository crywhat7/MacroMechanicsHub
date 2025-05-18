using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using MacroMechanicsHub.Models;
using Newtonsoft.Json;
using OpenAI.Chat;
using System;

namespace MacroMechanicsHub.Services
{
    public class AssistantService
    {

        private readonly LoLApiService _lolApiService = new LoLApiService();
        private readonly string apiKey = "NI PINGA";

        public string GetPromptForIA()
        {
            return @"Eres un asistente de macro para partidas de League of Legends, te daré de forma resumida la información de la partida y de cada campeón del juego,
                     tu trabajo es devolver en formato JSON de la siguiente forma:
                      {
                        'WhatToDoNext': [],
                        'Warnings': [],
                        'Advices' []
                      }
                     siendo el contenido de los arrays cadenas de texto simples (3)

                     Además, te proporcionaré una captura reciente del minimapa del juego, que puedes usar para mejorar tus respuestas. (si la imagen no es proporcionada o no tiene sentido, ignorala y de igual forma envía el resultado)

                     La respuesta debe estar adaptada especificamente para el campeón actual, el que estoy jugando, el que se caracteriza por ""isMe"",
                     deben ser consejos puntuales, acertados, no generales.

                     Devuelve la respuesta en formato JSON, sin ningún otro texto adicional, ni explicaciones, ni comentarios.

                     " + GetResumedDataForIA() + @"

                     Recuerda que el formato de salida debe ser JSON válido y no debe incluir ningún otro texto.
                    ";

        }

        private string GetResumedDataForIA(
            )
        {
            // 1. Obtener los datos de la API
            var allGameData = _lolApiService.GetAllGameData();

            if (allGameData == null)
            {
                return "No se pudo obtener la información del juego.";
            }

            // 1. Obtener los campeones y sus datos
            var champs = allGameData.allPlayers.Select(champ => new
            {
                name = champ.championName,
                champ.scores.kills,
                champ.scores.deaths,
                champ.scores.assists,
                cs = champ.scores.creepScore,
                isMe = champ.riotId == allGameData.activePlayer.riotId,
                items = champ.items.Select(item => new
                {
                    name = item.displayName
                }
                ).ToList()
            }).ToList();

            // 2. Obtener los eventos del juego

            var events = allGameData.events.Events.Select(evt => new
            {
                id = evt.EventID,
                evento = evt.EventName,
                killerName = evt.KillerName ?? "Is not a kill",
                victinName = evt.VictimName ?? "Is not a kill",
            });

            // 3. Devolver la información resumida como JSON (TEXTO)
            var sb = new StringBuilder();


            sb.AppendLine(
                "Aquí tienes la información de la partida y de cada campeón en formato JSON:");

            sb.AppendLine("{");
            sb.AppendLine($"\"campeones\": {JsonConvert.SerializeObject(champs)},");
            sb.AppendLine($"\"eventos\": {JsonConvert.SerializeObject(events)},");
            sb.AppendLine($"\"mapa\": \"{allGameData.gameData.mapName}\",");
            sb.AppendLine($"\"modo\": \"{allGameData.gameData.gameMode}\",");
            sb.AppendLine($"\"duracion\": \"{allGameData.gameData.gameTime}\",");
            sb.AppendLine($"\"tiempo_de_juego\": \"{allGameData.gameData.gameTime}\",");
            sb.AppendLine($"\"jugador_activo\": \"{allGameData.activePlayer.summonerName}\"");
            sb.AppendLine($",\"oro_actual\": \"{allGameData.activePlayer.currentGold}\"");
            sb.AppendLine("}");

            return sb.ToString();

        }

        public AssistantResponse GetAssistantResponse()
        {
            return JsonConvert.DeserializeObject<AssistantResponse>("{\r\n  \"WhatToDoNext\": [\r\n    \"Presiona tu ventaja con una cazada rápida: busca a los carries enemigos mal posicionados y ejecuta un combo flash+R+W+Q para eliminarlos instantáneamente.\",\r\n    \"Agrúpate con tu equipo para forzar peleas 5v4 sabiendo que tienes daño más que suficiente y zoneo con tu stun de Annie.\",\r\n    \"Empuja líneas secundarias solo si tienes visión profunda y tu equipo puede rotar rápido, priorizando top o bot para generar presión dividida.\"\r\n  ],\r\n  \"Warnings\": [\r\n    \"Evita sobreextenderte, ya tienes una racha perfecta y mucho oro: el respawn enemigo puede intentar cazar especialmente a Annie.\",\r\n    \"No pelees solo contra Irelia o Lillia si no tienes tu flash y ultimate disponibles, podrían sobrevivir al burst y darte la vuelta.\",\r\n    \"Ten cuidado con las emboscadas múltiples; tu muerte valdría demasiado oro y podría cambiar el ritmo de la partida.\"\r\n  ],\r\n  \"Advices\": [\r\n    \"Compra un Zhonya's para protegerte en caso de divear demasiado profundo con tu combo y sobrevivir al focus enemigo.\",\r\n    \"Usa control de visión agresivo con tus centinelas invisibles antes de iniciar, asegurando rutas de escape y evitando colapsos enemigos.\",\r\n    \"Mantén tu stun cargado antes de moverte por el mapa o pelear; ese control puede definir la victoria en teamfights.\"\r\n  ]\r\n}");
            string prompt = GetPromptForIA();

            ChatClient client = new ChatClient("gpt-4.1", apiKey);

            try
            {
                ChatCompletion completion = client.CompleteChat(prompt);

                string jsonResponse = completion.Content[0].Text;

                // Deserializar la respuesta JSON
                var assistantResponse = JsonConvert.DeserializeObject<AssistantResponse>(jsonResponse);

                return assistantResponse;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
