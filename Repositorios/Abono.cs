//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Prestamos.Repositorios
{
    using System;
    using System.Collections.Generic;
    
    public partial class Abono
    {
        public int IdAbono { get; set; }
        public int IdPago { get; set; }
        public decimal Valor { get; set; }
        public System.DateTime Fecha { get; set; }
    
        public virtual Pago Pago { get; set; }
    }
}
