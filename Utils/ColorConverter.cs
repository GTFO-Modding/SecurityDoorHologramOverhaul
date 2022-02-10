﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace SecurityDoorHologramOverhaul.Utils
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override bool HandleNull => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var color = new Color();
            float multiplier = 1.0f;

            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            return color * multiplier;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected PropertyName token");

                        var propName = reader.GetString();
                        reader.Read();

                        switch (propName.ToLower())
                        {
                            case "r":
                                color.r = reader.GetSingle();
                                break;

                            case "g":
                                color.g = reader.GetSingle();
                                break;

                            case "b":
                                color.b = reader.GetSingle();
                                break;

                            case "a":
                                color.a = reader.GetSingle();
                                break;

                            case "multiplier":
                                multiplier = reader.GetSingle();
                                break;
                        }
                    }
                    throw new JsonException("Expected EndObject token");

                case JsonTokenType.String:
                    var strValue = reader.GetString().Trim();
                    var strValues = strValue.Split("*");
                    string formatString;

                    switch (strValues.Length)
                    {
                        case 1:
                            multiplier = 1.0f;
                            formatString = strValues[0].Trim();
                            break;

                        case 2:
                            if (!float.TryParse(strValues[1].Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out multiplier))
                                throw new JsonException($"Color multiplier is not valid number! (*): {strValue}");
                            formatString = strValues[0].Trim();
                            break;

                        default:
                            throw new JsonException($"Color format has more than two Mutiplier (*): {strValue}");
                    }
                    if (ColorUtility.TryParseHtmlString(formatString, out color))
                    {
                        return color * multiplier;
                    }

                    if (TryParseColor(strValue, out color))
                    {
                        return color * multiplier;
                    }
                    throw new JsonException($"Color format is not right: {strValue}");

                default:
                    throw new JsonException($"ColorJson type: {reader.TokenType} is not implemented!");
            }
        }

        private static bool TryParseColor(string input, out Color color)
        {
            if (!RegexUtil.TryParseVectorString(input, out var array))
            {
                color = Color.white;
                return false;
            }

            if (array.Length < 3)
            {
                color = Color.white;
                return false;
            }

            float alpha = 1.0f;
            if (array.Length > 3)
            {
                alpha = array[3];
            }

            color = new Color(array[0], array[1], array[2], alpha);
            return true;
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("#" + ColorUtility.ToHtmlStringRGBA(value));
        }
    }
}
