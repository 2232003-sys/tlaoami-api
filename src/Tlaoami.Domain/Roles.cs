namespace Tlaoami.Domain
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Administrativo = "Administrativo";
        public const string Consulta = "Consulta";

        public static readonly string[] All = { Admin, Administrativo, Consulta };
        public const string AdminAndAdministrativo = "Admin,Administrativo";
        public const string AllRoles = "Admin,Administrativo,Consulta";
    }
}
