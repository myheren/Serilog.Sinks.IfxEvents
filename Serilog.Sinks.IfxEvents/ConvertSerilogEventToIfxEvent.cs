using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomSerilogSinks;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;

namespace Serilog.Sinks.IfxEvents
{
    public static class ConvertSerilogEventToIfxEvent
    {
        private static IDictionary<string, string> GetSimpleHashTable<T>(IReadOnlyDictionary<T, LogEventPropertyValue> props)
        {
            return props.ToDictionary(kvp => kvp.Key.ToString().Trim('"'), kvp => kvp.Value.ToString().Trim('"'), StringComparer.CurrentCultureIgnoreCase);
        }
        private static string GetProp(List<string> propNameLst, IDictionary<string, string> dict)
        {
            foreach (var propName in propNameLst)
            {
                if (dict.ContainsKey(propName))
                {
                    return dict[propName];
                }
            }
            return String.Empty;
        }
        private static long? GetLongProp(List<string> propNameLst, IDictionary<string, string> dict)
        {
            foreach (var propName in propNameLst)
            {
                if (dict.ContainsKey(propName))
                {
                    long res;
                    bool converted = Int64.TryParse(dict[propName], out res);
                    if (converted)
                    {
                        return res;
                    }
                }
            }
            return null;
        }
        private static string GetProp<T>(List<string> propNameLst, IReadOnlyDictionary<T, LogEventPropertyValue> props)
        {
            return GetProp(propNameLst, GetSimpleHashTable(props));
        }

        public static AppLogEvent GetIfxEvent(this LogEvent logEvent, ITextFormatter formatter)
        {
            AppLogEvent curEvent = new AppLogEvent
            {
                TraceLevel = (uint)logEvent.Level,
                MessageTemplate = logEvent.MessageTemplate.Text,
                TrackId = GetProp(new List<string> { "track_id", "TrackId" }, logEvent.Properties),
                //YYYY-MM-DDThh:mm:ss[.f{1,7}]Z
                //OriginalTime = logEvent.Timestamp.ToString("YYYY-MM-DDThh:mm:ss.fffZ")
                OriginalTimeFromSerilog = logEvent.Timestamp.ToString()
            };
            foreach (var token in logEvent.MessageTemplate.Tokens)
            {
                var complexToken = token as PropertyToken;
                if (complexToken != null)
                {
                    var tokenObj = logEvent.Properties[complexToken.PropertyName] as DictionaryValue;
                    if (tokenObj != null)
                    {
                        var dict = GetSimpleHashTable(tokenObj.Elements);
                        long? tmp = GetLongProp(new List<string> { "ConsumerId", "UserId" }, dict);
                        if (tmp != null) curEvent.ConsumerId = tmp.Value;
                        tmp = GetLongProp(new List<string> { "MerchantId" }, dict);
                        if (tmp != null) curEvent.MerchantId = tmp.Value;
                        if (String.IsNullOrWhiteSpace(curEvent.TrackId))
                        {
                            curEvent.TrackId = GetProp(new List<string> { "TrackId" }, dict);
                        }
                        curEvent.MsgType = GetProp(new List<string> { "Type" }, dict);
                        curEvent.MsgSource = GetProp(new List<string> { "Source" }, dict);
                        curEvent.MsgTarget = GetProp(new List<string> { "Target" }, dict);
                        curEvent.MessageId = GetProp(new List<string> { "MessageId" }, dict);
                    }
                }
            }
            using (var render = new StringWriter())
            {
                formatter.Format(logEvent, render);
                curEvent.Message = render.ToString();
            }
            return curEvent;
        }
    }
}
