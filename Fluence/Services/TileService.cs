using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Fluence.Services
{
    public class TileService
    {
        public static void UpdateLiveTile(double budgetPercent, double balance, double daily, double spentToday, double weekly, string topCategory)
        {
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueue(true);
                updater.Clear();

                updater.Update(CreateSimpleTile(balance.ToString("N2"), "current balance", "t1"));
                updater.Update(CreateSimpleTile($"{budgetPercent:N0}%", "monthly budget health", "t2"));
                updater.Update(CreateSimpleTile(daily.ToString("N2"), "daily allowance", "t3"));
                updater.Update(CreateSimpleTile(spentToday.ToString("N2"), "spent today", "t4"));
                updater.Update(CreateSimpleTile(weekly.ToString("N2"), "weekly total", "t5"));
            }
            catch (Exception) { }
        }

        private static TileNotification CreateSimpleTile(string value, string label, string tag)
        {
            XmlDocument wideXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text09);
            var wideText = wideXml.GetElementsByTagName("text");
            if (wideText.Length > 0) wideText[0].InnerText = value;
            if (wideText.Length > 1) wideText[1].InnerText = label.ToLower();

            XmlDocument squareXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            var squareText = squareXml.GetElementsByTagName("text");
            if (squareText.Length > 0) squareText[0].InnerText = value;
            if (squareText.Length > 1) squareText[1].InnerText = label.ToLower();

            var visual = wideXml.GetElementsByTagName("visual");
            if (visual != null && visual.Length > 0)
            {
                var bindingNode = squareXml.GetElementsByTagName("binding").Item(0);
                if (bindingNode != null)
                {
                    IXmlNode importedBinding = wideXml.ImportNode(bindingNode, true);
                    visual[0].AppendChild(importedBinding);
                }
            }

            return new TileNotification(wideXml)
            {
                Tag = tag,
                ExpirationTime = DateTimeOffset.Now.AddHours(24)
            };
        }
    }
}
