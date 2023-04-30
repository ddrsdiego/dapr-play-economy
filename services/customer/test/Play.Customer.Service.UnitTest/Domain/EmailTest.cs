namespace Play.Customer.Service.UnitTest.Domain;

using Core.Domain.AggregateModel.CustomerAggregate;
using FluentAssertions;
using NUnit.Framework;

public class EmailTest
{
    [TestCase("john.doe@example.com", true)] // E-mail válido
    [TestCase("john-doe@example.com", true)] // E-mail válido com hífen
    [TestCase("john_doe@example.com", true)] // E-mail válido com sublinhado
    [TestCase("john+doe@example.com", true)] // E-mail válido com sinal de mais
    [TestCase("john.doe@example.co.uk", true)] // E-mail válido com domínio de nível superior (TLD) de vários segmentos
    [TestCase("", false)] // E-mail vazio
    [TestCase("john.doe", false)] // E-mail sem '@'
    [TestCase("john.doe@", false)] // E-mail sem domínio
    [TestCase("@example.com", false)] // E-mail sem parte local
    [TestCase("john.doe@example", false)] // E-mail sem ponto no domínio
    [TestCase("john.doe@.com", false)] // E-mail com ponto no início do domínio
    [TestCase("john.doe@example..com", false)] // E-mail com dois pontos consecutivos no domínio
    [TestCase("john.doe@.example.com", false)] // E-mail com ponto no início do subdomínio
    [TestCase("john.doe@example.com.", false)] // E-mail com ponto no final do domínio
    [TestCase("john..doe@example.com", false)] // E-mail com dois pontos consecutivos na parte local
    public void TestEmailValidation(string email, bool expectedResult)
    {
        var emailResult = Email.Create(email);
        emailResult.IsSuccess.Should().Be(expectedResult);
    }
}