using Windows.Security.Credentials;
using FluentTlsTunnel.Core;
namespace FluentTlsTunnel;
static class PasswordStore
{
    static string Resource(Profile p)=>"FluentTlsTunnel:"+p.Endpoint;
    public static string Load(Profile p){try{var c=new PasswordVault().Retrieve(Resource(p),p.Username);c.RetrievePassword();return c.Password;}catch{return "";}}
    public static void Save(Profile p,string password){var v=new PasswordVault();try{v.Remove(v.Retrieve(Resource(p),p.Username));}catch{}if(password.Length>0)v.Add(new PasswordCredential(Resource(p),p.Username,password));}
}
