using System.ComponentModel;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace NLogEncryt
{
    [LayoutRenderer("Encrypt")]
    [AmbientProperty("Encrypt")]
    [ThreadAgnostic]
    public sealed class EncryptLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        public EncryptLayoutRendererWrapper()
        {
            Encrypt = true;
        }

        [DefaultValue(true)]
        public bool Encrypt { get; set; }

        protected override string Transform(string text)
        {
            return Encrypt ? LogEncryptor.EncryptData(text) : text;
        }
    }
}