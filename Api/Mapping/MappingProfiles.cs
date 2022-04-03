using AutoMapper;

namespace OhMyWord.Api.Mapping;

public static class MappingProfiles
{
    private static readonly Profile[] Profiles = 
    {
        new ResponsesProfile()
    };

    public static IEnumerable<Profile> GetProfiles() => Profiles.AsEnumerable();
}
