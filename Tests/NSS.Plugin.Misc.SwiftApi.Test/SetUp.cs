using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NUnit.Framework;

namespace NSS.Plugin.Misc.SwiftApi.Tests
{
    [SetUpFixture]
    public class SetUp
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // We need to create all the mappings before any of the test are run
            // All maps are created in the ApiMapperConfiguration constructor.
            ApiMapperConfiguration mapps = new ApiMapperConfiguration();
        }
    }
}
