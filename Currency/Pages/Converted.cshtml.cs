using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Currency.Pages;

namespace Currency.Pages
{
    public class ConvertedModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string From { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Quantity { get; set; }
        [BindProperty(SupportsGet = true)]
        public string To { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Result { get; set; }
        public void OnGet(string currencyFrom, string quantity, string to, string result)
        {
            To = to;
            //From = currencyFrom;
            Quantity = quantity;
            Result = result;
        }
    }
}
