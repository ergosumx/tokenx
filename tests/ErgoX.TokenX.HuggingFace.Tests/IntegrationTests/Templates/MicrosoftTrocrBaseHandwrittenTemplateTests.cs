namespace ErgoX.TokenX.HuggingFace.Tests.IntegrationTests.Templates
{
    using Xunit;

    public sealed class MicrosoftTrocrBaseHandwrittenTemplateTests
    {
        private const string ModelFolder = "microsoft-trocr-base-handwritten";

        [Fact]
        public void TokenizationEdgeLong()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-edge-long.json");
        }

        [Fact]
        public void TokenizationEdgeMedium()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-edge-medium.json");
        }

        [Fact]
        public void TokenizationEdgeShort()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-edge-short.json");
        }

        [Fact]
        public void TokenizationEdgeTiny()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-edge-tiny.json");
        }

        [Fact]
        public void TokenizationStandardLongDe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-de.json");
        }

        [Fact]
        public void TokenizationStandardLongEl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-el.json");
        }

        [Fact]
        public void TokenizationStandardLongEn()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-en.json");
        }

        [Fact]
        public void TokenizationStandardLongEs()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-es.json");
        }

        [Fact]
        public void TokenizationStandardLongFr()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-fr.json");
        }

        [Fact]
        public void TokenizationStandardLongHi()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-hi.json");
        }

        [Fact]
        public void TokenizationStandardLongIt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-it.json");
        }

        [Fact]
        public void TokenizationStandardLongJa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-ja.json");
        }

        [Fact]
        public void TokenizationStandardLongKa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-ka.json");
        }

        [Fact]
        public void TokenizationStandardLongKo()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-ko.json");
        }

        [Fact]
        public void TokenizationStandardLongNl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-nl.json");
        }

        [Fact]
        public void TokenizationStandardLongPl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-pl.json");
        }

        [Fact]
        public void TokenizationStandardLongPt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-pt.json");
        }

        [Fact]
        public void TokenizationStandardLongTa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-ta.json");
        }

        [Fact]
        public void TokenizationStandardLongTe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-te.json");
        }

        [Fact]
        public void TokenizationStandardLongZh()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-long-zh.json");
        }

        [Fact]
        public void TokenizationStandardMediumDe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-de.json");
        }

        [Fact]
        public void TokenizationStandardMediumEl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-el.json");
        }

        [Fact]
        public void TokenizationStandardMediumEn()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-en.json");
        }

        [Fact]
        public void TokenizationStandardMediumEs()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-es.json");
        }

        [Fact]
        public void TokenizationStandardMediumFr()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-fr.json");
        }

        [Fact]
        public void TokenizationStandardMediumHi()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-hi.json");
        }

        [Fact]
        public void TokenizationStandardMediumIt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-it.json");
        }

        [Fact]
        public void TokenizationStandardMediumJa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-ja.json");
        }

        [Fact]
        public void TokenizationStandardMediumKa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-ka.json");
        }

        [Fact]
        public void TokenizationStandardMediumKo()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-ko.json");
        }

        [Fact]
        public void TokenizationStandardMediumNl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-nl.json");
        }

        [Fact]
        public void TokenizationStandardMediumPl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-pl.json");
        }

        [Fact]
        public void TokenizationStandardMediumPt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-pt.json");
        }

        [Fact]
        public void TokenizationStandardMediumTa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-ta.json");
        }

        [Fact]
        public void TokenizationStandardMediumTe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-te.json");
        }

        [Fact]
        public void TokenizationStandardMediumZh()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-medium-zh.json");
        }

        [Fact]
        public void TokenizationStandardShortDe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-de.json");
        }

        [Fact]
        public void TokenizationStandardShortEl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-el.json");
        }

        [Fact]
        public void TokenizationStandardShortEn()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-en.json");
        }

        [Fact]
        public void TokenizationStandardShortEs()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-es.json");
        }

        [Fact]
        public void TokenizationStandardShortFr()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-fr.json");
        }

        [Fact]
        public void TokenizationStandardShortHi()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-hi.json");
        }

        [Fact]
        public void TokenizationStandardShortIt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-it.json");
        }

        [Fact]
        public void TokenizationStandardShortJa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-ja.json");
        }

        [Fact]
        public void TokenizationStandardShortKa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-ka.json");
        }

        [Fact]
        public void TokenizationStandardShortKo()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-ko.json");
        }

        [Fact]
        public void TokenizationStandardShortNl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-nl.json");
        }

        [Fact]
        public void TokenizationStandardShortPl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-pl.json");
        }

        [Fact]
        public void TokenizationStandardShortPt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-pt.json");
        }

        [Fact]
        public void TokenizationStandardShortTa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-ta.json");
        }

        [Fact]
        public void TokenizationStandardShortTe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-te.json");
        }

        [Fact]
        public void TokenizationStandardShortZh()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-short-zh.json");
        }

        [Fact]
        public void TokenizationStandardTinyDe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-de.json");
        }

        [Fact]
        public void TokenizationStandardTinyEl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-el.json");
        }

        [Fact]
        public void TokenizationStandardTinyEn()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-en.json");
        }

        [Fact]
        public void TokenizationStandardTinyEs()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-es.json");
        }

        [Fact]
        public void TokenizationStandardTinyFr()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-fr.json");
        }

        [Fact]
        public void TokenizationStandardTinyHi()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-hi.json");
        }

        [Fact]
        public void TokenizationStandardTinyIt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-it.json");
        }

        [Fact]
        public void TokenizationStandardTinyJa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-ja.json");
        }

        [Fact]
        public void TokenizationStandardTinyKa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-ka.json");
        }

        [Fact]
        public void TokenizationStandardTinyKo()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-ko.json");
        }

        [Fact]
        public void TokenizationStandardTinyNl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-nl.json");
        }

        [Fact]
        public void TokenizationStandardTinyPl()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-pl.json");
        }

        [Fact]
        public void TokenizationStandardTinyPt()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-pt.json");
        }

        [Fact]
        public void TokenizationStandardTinyTa()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-ta.json");
        }

        [Fact]
        public void TokenizationStandardTinyTe()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-te.json");
        }

        [Fact]
        public void TokenizationStandardTinyZh()
        {
            TemplateTestUtilities.AssertTemplateCase(ModelFolder, "tokenization-standard-tiny-zh.json");
        }

    }
}

