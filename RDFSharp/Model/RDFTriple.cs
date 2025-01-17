﻿/*
   Copyright 2012-2022 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using RDFSharp.Query;
using System;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTriple represents a triple in the RDF model.
    /// </summary>
    public class RDFTriple : IEquatable<RDFTriple>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the triple
        /// </summary>
        internal long TripleID => LazyTripleID.Value;
        private readonly Lazy<long> LazyTripleID;

        /// <summary>
        /// Flavor of the triple
        /// </summary>
        public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; internal set; }

        /// <summary>
        /// Member acting as subject token of the triple
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate token of the triple
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object token of the triple
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        /// <summary>
        /// Subject of the triple's reification
        /// </summary>
        public RDFResource ReificationSubject => LazyReificationSubject.Value;
        private readonly Lazy<RDFResource> LazyReificationSubject;
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFResource obj)
        {
            if (pred == null)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");

            this.TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
            this.Subject = subj ?? new RDFResource();
            this.Predicate = pred;
            this.Object = obj ?? new RDFResource();
            this.LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.ToString()));
            this.LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", this.TripleID.ToString())));
        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFTriple(RDFResource subj, RDFResource pred, RDFLiteral lit)
        {
            if (pred == null)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFModelException("Cannot create RDFTriple because \"pred\" parameter is a blank resource");

            this.TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
            this.Subject = subj ?? new RDFResource();
            this.Predicate = pred;
            this.Object = lit ?? new RDFPlainLiteral(string.Empty);
            this.LazyTripleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.ToString()));
            this.LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", this.TripleID.ToString())));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the triple
        /// </summary>
        public override string ToString()
            => string.Concat(this.Subject.ToString(), " ", this.Predicate.ToString(), " ", this.Object.ToString());

        /// <summary>
        /// Performs the equality comparison between two triples
        /// </summary>
        public bool Equals(RDFTriple other)
            => other != null && this.TripleID.Equals(other.TripleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification graph of the triple
        /// </summary>
        public RDFGraph ReifyTriple()
        {
            RDFGraph reifGraph = new RDFGraph();

            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)this.Subject));
            reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)this.Predicate));
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)this.Object));
            else
                reifGraph.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)this.Object));

            return reifGraph;
        }
        #endregion
    }
}