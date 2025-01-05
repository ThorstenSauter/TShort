using Asp.Versioning.Conventions;
using FastEndpoints.AspVersioning;

namespace TShort.Api.Versioning;

public static class Versions
{
    internal const string ManagementApi = "Management";
    internal const double V1 = 1.0;

    public static void Register() =>
        VersionSets.CreateApi(ManagementApi, v => v.HasApiVersion(V1));
}
