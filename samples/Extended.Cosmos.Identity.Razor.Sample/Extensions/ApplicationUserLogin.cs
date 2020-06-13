﻿using Mobsites.Cosmos.Identity;

namespace Extended.Cosmos.Identity.Razor.Sample.Extensions
{
    public class ApplicationUserLogin : IdentityUserLogin
    {
        // Partition Key Path
        public string Discriminator => nameof(ApplicationUserLogin);

        // Override Base property and assign correct Partition Key value.
        public override string PartitionKey => Discriminator;

        // Any other properties to extend the default Cosmos identity model
    }
}
