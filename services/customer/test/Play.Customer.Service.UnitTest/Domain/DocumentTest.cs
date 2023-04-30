namespace Play.Customer.Service.UnitTest.Domain;

using Core.Domain.AggregateModel.CustomerAggregate;
using FluentAssertions;
using NUnit.Framework;

public class DocumentTest
{
    [SetUp]
    public void Setup()
    {
    }
    
    [TestCase("567.032.442-00   ", true)] // CPF válido
    [TestCase("529.982.247-25", true)] // CPF válido
    [TestCase("52998224725", true)] // CPF válido sem pontuação
    [TestCase("000.000.000-00", false)] // CPF inválido (dígitos repetidos)
    [TestCase("111.111.111-11", false)] // CPF inválido (dígitos repetidos)
    [TestCase("123.456.789-00", false)] // CPF inválido (números aleatórios)
    [TestCase("529.982.247-24", false)] // CPF inválido (dígito verificador incorreto)
    [TestCase("5299822472", false)] // CPF inválido (tamanho incorreto)
    [TestCase("529982247251", false)] // CPF inválido (tamanho incorreto)
    public void Should_Validate_Document(string document, bool expectedResult)
    {
        var sut =  Document.Create(document);
        sut.IsSuccess.Should().Be(expectedResult);
    }
}