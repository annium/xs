namespace Xs.Registry.Dotnet.Views
{
    internal class RegistrationIndexView
    {
        public int Count => Items.Length;

        public RegistrationPageView[] Items { get; }

        public RegistrationIndexView(
            RegistrationPageView[] items
        )
        {
            Items = items;
        }
    }
}