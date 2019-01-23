namespace Xs.Registry.Core.Auth
{
    public enum Access
    {
        // access is allowed through api
        Api = 1,
        // access is allowed through session-based client (web site)
        Session = 2
    }
}