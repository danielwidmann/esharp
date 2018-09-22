using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsTarget;
using Mono.Cecil;

namespace ESharp.Optimizations.File
{
	class TypeEnumGenerator
	{
		static public void FileOptimization(List<EcsFile> files, FileOptimizationContext context)
		{
			files.Add(new EcsFile {
				Content = WriteTypesEnum(context.UsedTypes), Name = "TypeIds.h", Override = true
			});
			files.Add(new EcsFile {
				Content = WriteTypesEnumSource(context.UsedTypes),
				Name = "TypeIds.c",
				Override = true
			});
		}

		static string WriteTypesEnum(IEnumerable<TypeDefinition> types)
		{

			var nonStaticTypes = types.Where(x => !(x.IsAbstract)); //x.IsSealed && 
			StringBuilder w = new StringBuilder();
			{
				w.AppendLine("#pragma once");
				w.AppendLine("// pre-header");
				w.AppendLine("// internal includes");
				w.AppendLine("// external includes");
				w.AppendLine("// header");

				w.AppendLine("enum {");
				foreach (var t in nonStaticTypes) {
					w.AppendLine(t.Name + "_TypeId,");
				}

				w.AppendLine("};");

				foreach (var t in nonStaticTypes) {
					w.AppendLine("#define " + t.Name.ToUpper() + "_USED");
				}


				w.AppendLine("extern const unsigned char TypeSizes[];");
				w.AppendLine("extern const char* TypeNames[];");
				w.AppendLine("extern const int type_count;");

				w.AppendLine("#include <stddef.h>");
				w.AppendLine("typedef struct {uint16_t etypeid; uint16_t offset;} type_meta_t;");
				w.AppendLine("extern const type_meta_t TypeMeta[];");
				w.AppendLine("extern const int type_meta_count;");        			
			}
			return w.ToString();
		}

		static string WriteTypesEnumSource(IEnumerable<TypeDefinition> types)
		{
			var nonStaticTypes = types.Where(x => !( x.IsAbstract)); //x.IsSealed &&
			StringBuilder w = new StringBuilder();
			{

				w.AppendLine("const unsigned char TypeSizes[] = {");
				foreach (var t in nonStaticTypes) {
					w.AppendLine("sizeof(" + t.Name + "_struct),");
				}
				w.AppendLine("};");

				w.AppendLine("const int type_count = sizeof(TypeSizes) / sizeof(TypeSizes[0]);");

				w.AppendLine("const char* TypeNames[] = {");
				foreach (var t in nonStaticTypes) {
					w.AppendLine("\"" + t.Name + "\",");
				}
				w.AppendLine("};");

				w.AppendLine("const type_meta_t TypeMeta[] = {");

				var metaDataFields = nonStaticTypes.SelectMany(x => x.Fields).Where(f => !f.IsStatic && !f.FieldType.IsValueType);
				foreach (var f in metaDataFields) {

					w.AppendLine("{" + f.DeclaringType.Name + "_TypeId, offsetof(" + f.DeclaringType.Name + "_struct, " + f.Name + ")}, // " + f.FieldType.Name);
					
				}

				if(metaDataFields.Count() == 0) {
					w.AppendLine("{EObject_TypeId, 0} // dummy entry to make compiler happy");
				}	

				w.AppendLine("};");

				w.AppendLine("const int type_meta_count = sizeof(TypeMeta) / sizeof(TypeMeta[0]);");

			}			

			return w.ToString();
		}

	}
}
