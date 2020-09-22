using System.Collections.Generic;
using System.Linq;
using Moq;
using Nop.Data;
using Nop.Plugin.Swift.Api.Domain.Customers;
using Nop.Plugin.Swift.Api.Services;
using NUnit.Framework;

namespace Nop.Plugin.Api.Tests.ServicesTests.Companies
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
                            Name = "Test 1",
                            SalesContactEmail = "Contact@teest.com",
                            SalesContactLiveChatId = "ChatId1",
                            SalesContactName = "Contact Test",
                            SalesContactPhone = "1234567890"
                        },
                        new Company()
                        {
                            Id = 2,
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
            var companyResult = _companyApiService.GetCompanyEntityById(1);

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
            var companyResult = _companyApiService.GetCompanyEntityById(10);

            Assert.IsNull(companyResult);
        }
    }
}