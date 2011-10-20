using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment.Writing
{
    public class HostWriter
    {
        private readonly ITypeDescriptorCache _types;
        private readonly FlatFileWriter _writer = new FlatFileWriter(new List<string>());
        private readonly IFileSystem _fileSystem = new FileSystem();

        public HostWriter(ITypeDescriptorCache types)
        {
            _types = types;
        }

        public void WriteTo(string recipeName, HostDefinition host, DeploymentSettings settings)
        {
            var fileName = settings.GetHost(recipeName, host.Name);

            host.References.Each(WriteReference);
            host.Values.Each(WritePropertyValue);
            host.Directives.Each(WriteDirective);

            _fileSystem.WriteStringToFile(fileName, ToText());
        }

        public void WriteReference(BottleReference reference)
        {
            _writer.WriteLine(reference.ToString());
        }

        public void WriteDirective(IDirective directive)
        {
            var directiveWriter = new DirectiveWriter(_writer, _types);
            directiveWriter.Write(directive);
        }

        public string ToText()
        {
            return _writer.ToString();
        }

        public IEnumerable<string> AllLines()
        {
            return _writer.List;
        }

        public void WritePropertyValue(PropertyValue value)
        {
            _writer.WriteLine(value.ToString());
        }
    }
}