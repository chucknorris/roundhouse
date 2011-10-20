using System.Collections.Generic;
using FubuCore;

namespace Bottles.Deployment.Writing
{
    public class ProfileWriter
    {
        public void WriteTo(ProfileDefinition profile, DeploymentSettings settings)
        {
            var filename = settings.ProfileFileNameFor(profile.Name);
            new FileSystem().WriteToFlatFile(filename, writer =>
            {
                profile.Recipes.Each(r => writer.WriteLine(Profile.RecipePrefix + r));

                profile.ProfileDependencies.Each(pd => writer.WriteLine(Profile.ProfileDependencyPrefix + pd));

                profile.Values.Each(v => v.Write(writer));
            });
        }
    }
}