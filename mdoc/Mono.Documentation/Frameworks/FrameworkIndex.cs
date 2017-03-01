﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Mono.Cecil;

namespace Mono.Documentation
{

	class FrameworkIndex
	{
		List<FrameworkEntry> frameworks = new List<FrameworkEntry> ();
		string path;

		public FrameworkIndex (string pathToFrameworks) {
			path = pathToFrameworks;
		}

		public IList<FrameworkEntry> Frameworks {
			get {
				return this.frameworks;
			}
		}

		public FrameworkEntry StartProcessingAssembly (AssemblyDefinition assembly) {
			if (string.IsNullOrWhiteSpace (this.path))
				return FrameworkEntry.Empty;

			string assemblyPath = assembly.MainModule.FileName;
			string relativePath = assemblyPath.Replace (this.path, string.Empty);
			string shortPath = Path.GetDirectoryName (relativePath);
			if (shortPath.StartsWith (Path.DirectorySeparatorChar.ToString (), StringComparison.InvariantCultureIgnoreCase))
				shortPath = shortPath.Substring (1, shortPath.Length - 1);
			

			var entry = frameworks.FirstOrDefault (f => f.Name.Equals (shortPath));
			if (entry == null) {
				entry = new FrameworkEntry { Name = shortPath };
				frameworks.Add (entry);
			}
			return entry;
		}

		public void Write (string path) {
			if (string.IsNullOrWhiteSpace (this.path))
				return;
			
			string outputPath = Path.Combine (path, "FrameworksIndex");
			if (!Directory.Exists (outputPath))
				Directory.CreateDirectory (outputPath);
			
			foreach (var fx in this.frameworks) {

				XDocument doc = new XDocument (
					new XElement("Framework",
						new XAttribute ("Name", fx.Name),
				             fx.Types.Select(t => new XElement("Type",
                                   new XAttribute("Name", t.Name),
                                   t.Members.Select(m => 
	                                	new XElement("Member", 
			                                 new XAttribute("Sig", m)))))));

				// now save the document
				string filePath = Path.Combine (outputPath, fx.Name + ".xml");

				if (File.Exists (filePath))
					File.Delete (filePath);

				var settings = new XmlWriterSettings { Indent = true };
				using (var writer = XmlWriter.Create (filePath, settings)) {
					doc.WriteTo (writer);
				}
			}
		}

	}
}
