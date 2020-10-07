using System.Collections.Generic;
using System.Linq;
using Moq;
using Nop.Data;
using NSS.Plugin.Misc.SwiftApi.Services;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Services;
using NUnit.Framework;

namespace NSS.Plugin.Misc.SwiftApi.Tests.ServicesTests.Companies
{
    [TestFixture]
    public class CompanyServiceTests
    {
        private ICompanyService _companyApiService;

        [SetUp]
        public new void SetUp()
        {
            var companyRepository = new Mock<IRepository<Company>>();
            companyRepository.Setup(c => c.Table).Returns(new List<Company>()
                    {
                        new Company()
                        {
                            Id = 1,
                            ErpCompanyId = 100,
                            Name = "Test 1",
                            SalesContactEmail = "Contact@teest.com",
                            SalesContactLiveChatId = "ChatId1",
                            SalesContactName = "Contact Test",
                            SalesContactPhone = "1234567890"
                        },
                        new Company()
                        {
                            Id = 2,
                            ErpCompanyId = 2,
                            Name = "Test 2",
                            SalesContactEmail = "Contact2@teest.com",
                            SalesContactLiveChatId = "ChatId2",
                            SalesContactName = "Contact2 Test",
                            SalesContactPhone = "1234567891"
                        }

                    }.AsQueryable());

            _companyApiService = new CompanyService(companyRepository.Object);
        }

        [Test]
        public void GetCompanyById()
        {
            var companyResult = _companyApiService.GetCompanyEntityByErpEntityId(100);

            Assert.IsNotNull(companyResult);
            Assert.AreEqual(1, companyResult.Id);
            Assert.AreEqual("Test 1", companyResult.Name);
            Assert.AreEqual("Contact@teest.com", companyResult.SalesContactEmail);
            Assert.AreEqual("ChatId1", companyResult.SalesContactLiveChatId);
            Assert.AreEqual("Contact Test", companyResult.SalesContactName);
            Assert.AreEqual("1234567890", companyResult.SalesContactPhone);
        }

        [Test]
        public void GetCompanyById_NonExistantId()
        {
            var companyResult = _companyApiService.GetCompanyEntityByErpEntityId(10);

            Assert.IsNull(companyResult);
        }
    }
}