﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnSpy.Contracts.Highlighting;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Languages;
using dnSpy.Contracts.TreeView;
using dnSpy.NRefactory;
using dnSpy.Shared.UI.Files.TreeView;
using dnSpy.Shared.UI.Highlighting;
using ICSharpCode.Decompiler;

namespace dnSpy.Analyzer.TreeNodes {
	class MethodNode : EntityNode {
		readonly MethodDef analyzedMethod;
		readonly bool hidesParent;

		public MethodNode(MethodDef analyzedMethod, bool hidesParent = false) {
			if (analyzedMethod == null)
				throw new ArgumentNullException("analyzedMethod");
			this.analyzedMethod = analyzedMethod;
			this.hidesParent = hidesParent;
		}

		public override void Initialize() {
			this.TreeNode.LazyLoading = true;
		}

		protected override ImageReference GetIcon(IDotNetImageManager dnImgMgr) {
			return dnImgMgr.GetImageReference(analyzedMethod);
		}

		protected override void Write(ISyntaxHighlightOutput output, ILanguage language) {
			if (hidesParent) {
				output.Write("(", TextTokenType.Operator);
				output.Write("hides", TextTokenType.Text);
				output.Write(")", TextTokenType.Operator);
				output.WriteSpace();
			}
			language.WriteType(output, analyzedMethod.DeclaringType, true);
			output.Write(".", TextTokenType.Operator);
			new NodePrinter().Write(output, language, analyzedMethod, Context.ShowToken);
		}

		public override IEnumerable<ITreeNodeData> CreateChildren() {
			if (analyzedMethod.HasBody)
				yield return new MethodUsesNode(analyzedMethod);

			if (analyzedMethod.IsVirtual && !(analyzedMethod.IsNewSlot && analyzedMethod.IsFinal))
				yield return new VirtualMethodUsedByNode(analyzedMethod);
			else
				yield return new MethodUsedByNode(analyzedMethod);

			if (MethodOverridesNode.CanShow(analyzedMethod))
				yield return new MethodOverridesNode(analyzedMethod);

			if (InterfaceMethodImplementedByNode.CanShow(analyzedMethod))
				yield return new InterfaceMethodImplementedByNode(analyzedMethod);
		}

		public override IMemberRef Member {
			get { return analyzedMethod; }
		}

		public override IMDTokenProvider Reference {
			get { return analyzedMethod; }
		}
	}
}