﻿using System;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using NNS.Authentication.OAuth2.Exceptions;
using NUnit.Framework;

namespace NNS.Authentication.OAuth2.UnitTests
{
    public class ResourceOwnerTests
    {
        [Test]
        public void CreateResourceOwner()
        {
            ResourceOwners.CleanUpForTests();
            var resourceOwner = ResourceOwners.Add("user1");
            Assert.IsNotNull(resourceOwner);
            Assert.AreEqual("user1", resourceOwner.Name);

        }

        [Test]
        [ExpectedException(typeof(UserAlredyExistsException))]
        public void CreateResourceOwnerDouble()
        {
            ResourceOwners.CleanUpForTests();
            var resourceOwner = ResourceOwners.Add("user1");
            Assert.IsNotNull(resourceOwner);
            Assert.AreEqual("user1", resourceOwner.Name);

            var resourceOwner2 = ResourceOwners.Add("user1");
        }

        [Test]
        public void GetResourceOwner()
        {
            ResourceOwners.CleanUpForTests();
            ResourceOwners.Add("user1");
            ResourceOwners.Add("user2");

            var resourceOwner = ResourceOwners.GetResourceOwner("user1");
            resourceOwner.Name.Should().Be("user1");
            ResourceOwners.GetResourceOwner(resourceOwner.Guid).Should().NotBeNull();

            var resourceOwnerNull = ResourceOwners.GetResourceOwner("foo");
            resourceOwnerNull.Should().BeNull();
            ResourceOwners.GetResourceOwner(Guid.NewGuid()).Should().BeNull();


        }

        [Test]
        public void DisposeAndLoad()
        {
            ResourceOwners.CleanUpForTests();
            ResourceOwners.Add("user1");
            ResourceOwners.Add("user2");

            ResourceOwners.SaveToIsoStore();
            ResourceOwners.LoadFromIsoStore();

            var resourceOwner = ResourceOwners.GetResourceOwner("user1");
            Assert.IsNotNull(resourceOwner);
            Assert.AreEqual("user1", resourceOwner.Name);

            var resourceOwnerNull = ResourceOwners.GetResourceOwner("foo");
            Assert.IsNull(resourceOwnerNull);

        }

        [Test]
        public void ResourceOwnerToXElement()
        {
            var resourceOwner = new ResourceOwner("user1");
            var element = resourceOwner.ToXElement();

            element.Should().NotBeNull();
            element.Element("name").Should().NotBeNull();
            element.Element("name").Value.Should().Be("user1");
            element.Element("guid").Should().NotBeNull();
            element.Element("guid").Value.Should().Be(resourceOwner.Guid.ToString());
        }

        [Test]
        public void ResourceOwnerFromXElement()
        {
            var element = new XElement("ResourceOwner", new XElement("name", "user1"), new XElement("guid", "99c33d15-5fc1-417c-ae4e-0df51621c874"));
            var resourceOwner = ResourceOwner.FromXElement(element);

            resourceOwner.Should().NotBeNull();
            resourceOwner.Name.Should().Be("user1");
            resourceOwner.Guid.ToString().Should().Be("99c33d15-5fc1-417c-ae4e-0df51621c874");
        }

        [Test]
        public void AuthorizesMeToAccessTo()
        {
            var resourceOwner1 = ResourceOwners.Add("testusertoken1");
            var resourceOwner2 = ResourceOwners.Add("testusertoken2");

            var authorizationRequestUri = new Uri("http://example.com/TokenTest/AuthRequest");
            var accessTokenRequestUri = new Uri("http://example.com/TokenTest/AccessRequest");
            var redirectUri = new Uri("http://example.com/TokenTest/Redirect");
            var server = ServersWithAuthorizationCode.Add("testclienid", "testsecret", authorizationRequestUri,accessTokenRequestUri, redirectUri);

            var token = new Token(server, resourceOwner1);
            token.AuthorizationCode = "foobar";
            Tokens.AddToken(token);

            resourceOwner1.AuthorizesMeToAccessTo(server).Should().BeTrue();
            resourceOwner2.AuthorizesMeToAccessTo(server).Should().BeFalse();

        }

        [Test]
        public void GetSignedRequestTest()
        {
            var resourceOwner = ResourceOwners.Add("testusersignedRequest");
            var server = ServersWithAuthorizationCode.Add("clientid",
                                                          "secret",
                                                          new Uri("http://example.org/auth"),
                                                          new Uri("http://example.org/access"),
                                                          new Uri("http://example.org/redirect"));
            var token = new Token(server, resourceOwner);
            Tokens.AddToken(token);
            token.AuthorizationCode = "authcode";
            token.AccessToken = "access123";
            token.Expires = DateTime.Now.AddHours(1);
            
            var location1 = "http://example.org/protectedresource1";
            var location2 = "http://example.org/protectedresource2?foo=bar";

            var webRequest1 = resourceOwner.GetSignedRequestFor(server, location1);
            var webRequest2 = resourceOwner.GetSignedRequestFor(server, location2);

            webRequest1.RequestUri.Should().Be(location1 + "?access_token=access123");
            webRequest2.RequestUri.Should().Be(location2 + "&access_token=access123");
        }
    }
}
