using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using System.Collections;
using System.Resources;

namespace Common.Payload;

public class AssemblyEditor
{
    public static byte[] ReplaceRessources(byte[] assemblyBytes, Dictionary<string, object> newRessources)
    {
        using (var assStream = new MemoryStream(assemblyBytes))
        {
            var assemblyDef = AssemblyDefinition.ReadAssembly(assStream);
            var resources = assemblyDef.MainModule.Resources;
            var modules = assemblyDef.Modules;


            var existingRessource = resources.First();
            if (existingRessource.ResourceType != ResourceType.Embedded)
                return null;

            var emb = existingRessource as EmbeddedResource;
            var stream = emb.GetResourceStream();

            using (var reader = new ResourceReader(stream))
            {
                using (var ms = new MemoryStream())
                {

                    IDictionaryEnumerator dict = reader.GetEnumerator();
                    var writer = new ResourceWriter(ms);
                    while (dict.MoveNext())
                    {
                        var key = dict.Key.ToString();
                        if (newRessources.ContainsKey(key))
                            writer.AddResource(key, newRessources[key]);
                        else
                            writer.AddResource(key, dict.Value);
                    }

                    //writer.AddResource("Content", "Replaced!");
                    //writer.AddResource("ContentFile", newContent);
                    writer.Generate();
                    ms.Seek(0, SeekOrigin.Begin);

                    var newResource = new EmbeddedResource(existingRessource.Name, existingRessource.Attributes, ms);
                    resources.Remove(existingRessource);
                    resources.Add(newResource);

                    using (var outStream = new MemoryStream())
                    {
                        assemblyDef.Write(outStream);
                        outStream.Seek(0, SeekOrigin.Begin);
                        return outStream.ToArray();
                    }


                }
            }
        }


    }

    public static byte[] ChangeName(byte[] assemblyBytes, string name)
    {
        using (var assStream = new MemoryStream(assemblyBytes))
        {
            var assemblyDef = AssemblyDefinition.ReadAssembly(assStream);
            assemblyDef.Name = new AssemblyNameDefinition(name, Version.Parse("2.5.7.32"));


            using (var outStream = new MemoryStream())
            {
                assemblyDef.Write(outStream);
                outStream.Seek(0, SeekOrigin.Begin);
                return outStream.ToArray();
            }
        }
    }
}
