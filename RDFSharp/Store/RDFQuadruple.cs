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

using RDFSharp.Model;
using RDFSharp.Query;
using System;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFQuadruple represents a quadruple in the RDF store.
    /// </summary>
    public class RDFQuadruple : IEquatable<RDFQuadruple>
    {
        #region Properties
        /// <summary>
        /// Unique representation of the quadruple
        /// </summary>
        public long QuadrupleID => LazyQuadrupleID.Value;
        private readonly Lazy<long> LazyQuadrupleID;

        /// <summary>
        /// Flavor of the triple nested into the quadruple
        /// </summary>
        public RDFModelEnums.RDFTripleFlavors TripleFlavor { get; internal set; }

        /// <summary>
        /// Member acting as context token of the quadruple
        /// </summary>
        public RDFPatternMember Context { get; internal set; }

        /// <summary>
        /// Member acting as subject token of the quadruple
        /// </summary>
        public RDFPatternMember Subject { get; internal set; }

        /// <summary>
        /// Member acting as predicate token of the quadruple
        /// </summary>
        public RDFPatternMember Predicate { get; internal set; }

        /// <summary>
        /// Member acting as object token of the quadruple
        /// </summary>
        public RDFPatternMember Object { get; internal set; }

        /// <summary>
        /// Subject of the quadruple's reification
        /// </summary>
        public RDFResource ReificationSubject => LazyReificationSubject.Value;
        private readonly Lazy<RDFResource> LazyReificationSubject;
        #endregion

        #region Ctors
        /// <summary>
        /// SPO-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFResource obj)
        {
            if (pred == null)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");

            this.TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPO;
            this.Context = context ?? new RDFContext();
            this.Subject = subj ?? new RDFResource();
            this.Predicate = pred;
            this.Object = obj ?? new RDFResource();
            this.LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.ToString()));
            this.LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", this.QuadrupleID.ToString())));
        }

        /// <summary>
        /// SPL-flavor ctor
        /// </summary>
        public RDFQuadruple(RDFContext context, RDFResource subj, RDFResource pred, RDFLiteral lit)
        {
            if (pred == null)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is null");
            if (pred.IsBlank)
                throw new RDFStoreException("Cannot create RDFQuadruple because given \"pred\" parameter is a blank resource");

            this.TripleFlavor = RDFModelEnums.RDFTripleFlavors.SPL;
            this.Context = context ?? new RDFContext();
            this.Subject = subj ?? new RDFResource();
            this.Predicate = pred;
            this.Object = lit ?? new RDFPlainLiteral(string.Empty);
            this.LazyQuadrupleID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.ToString()));
            this.LazyReificationSubject = new Lazy<RDFResource>(() => new RDFResource(string.Concat("bnode:", this.QuadrupleID.ToString())));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the quadruple
        /// </summary>
        public override string ToString()
            => string.Concat(this.Context.ToString(), " ", this.Subject.ToString(), " ", this.Predicate.ToString(), " ", this.Object.ToString());

        /// <summary>
        /// Performs the equality comparison between two quadruples
        /// </summary>
        public bool Equals(RDFQuadruple other)
            => other != null && this.QuadrupleID.Equals(other.QuadrupleID);
        #endregion

        #region Methods
        /// <summary>
        /// Builds the reification store of the quadruple
        /// </summary>
        public RDFMemoryStore ReifyQuadruple()
        {
            RDFMemoryStore reifStore = new RDFMemoryStore();
            
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, this.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, this.ReificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)this.Subject));
            reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, this.ReificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)this.Predicate));
            if (this.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, this.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)this.Object));
            else
                reifStore.AddQuadruple(new RDFQuadruple((RDFContext)this.Context, this.ReificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)this.Object));

            return reifStore;
        }
        #endregion
    }
}