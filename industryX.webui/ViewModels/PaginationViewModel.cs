namespace IndustryX.WebUI.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<int> PageSizeOptions { get; set; } = new() { 10, 20, 50, 100, 200 };

        public string? ActionName { get; set; }
        public string? ControllerName { get; set; }
        public Dictionary<string, string?> RouteValues { get; set; } = new();

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
