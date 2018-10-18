using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using MarginToPaddingConverter.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarginToPaddingConverter.Controllers
{
    public class ConverterController : Controller
    {
        public IActionResult Index()
        {
            return View(new ConverterModel());
        }

        [HttpPost]
        public IActionResult Index(ConverterModel model)
        {
            try
            {
                var card = AdaptiveCard.FromJson(model.SourceCard).Card;

                Convert(card, card.Body);
                model.ConvertedCard = card.ToJson();
            }
            catch
            {
                model.ConvertedCard = "Error, maybe invalid card?";
            }

            return View(model);
        }

        private static void Convert(AdaptiveTypedElement parent, IEnumerable<AdaptiveElement> children)
        {
            // Recursively convert
            foreach (var el in children)
            {
                if (el is AdaptiveContainer container)
                {
                    Convert(container, container.Items);
                }
                else if (el is AdaptiveColumnSet columnSet)
                {
                    foreach (var col in columnSet.Columns)
                    {
                        Convert(col, col.Items);
                    }
                }
            }

            // If any children have margin
            bool childrenHaveMargin = children.Any(i => HasMargin(i));
            if (childrenHaveMargin)
            {
                parent.AdditionalProperties["padding"] = "none";

                bool shouldSkipTopPadding = false;
                foreach (var child in children)
                {
                    if (HasMargin(child))
                    {
                        // Next element should skip top padding
                        shouldSkipTopPadding = true;
                    }
                    else
                    {
                        if (shouldSkipTopPadding)
                        {
                            dynamic padding = new ExpandoObject();
                            padding.top = "none";
                            padding.left = "default";
                            padding.bottom = "default";
                            padding.right = "default";
                            child.AdditionalProperties["padding"] = padding;
                        }
                        else
                        {
                            child.AdditionalProperties["padding"] = "default";
                        }
                    }
                }
            }
        }

        private static bool HasMargin(AdaptiveElement element)
        {
            return element.AdditionalProperties.ContainsKey("margin");
        }
    }
}