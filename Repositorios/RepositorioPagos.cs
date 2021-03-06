﻿using System;
using System.Collections.Generic;
using System.Linq;
using Prestamos.Repositorios;
using Prestamos.Modelos;



namespace Prestamos.Repositorios
{
    [Serializable]
    public class RepositorioPagos
    {

        public void GuardarPago(Pago pago, Prestamo presta)
        {
           using (var context = new PrestamosEntities())
            {
                var Abono = pago.ValorPago;
                var IDPagoAbono = pago.IDPago;
                var pres = context.Prestamo.FirstOrDefault(cl => cl.NoPrestamo == presta.NoPrestamo);
                var pag = context.Pago.FirstOrDefault(x => x.IDPago == pago.IDPago);

                if (pago.ValorPago < pag.ValorPago)
                {
                    var pc = new PagoCuota();
                    pc = new PagoCuota();
                    pc.IDPago = pago.IDPago;
                    pc.Valor = pago.ValorPago;
                    pc.FechaPago = pago.FechaPago;
                    context.PagoCuota.Add(pc);

                    var pp = new PrestamoPago();
                    pp = new PrestamoPago();
                    pp.NoPrestamo = presta.NoPrestamo;
                    context.PrestamoPago.Add(pp);

                    int id = pp.PrestamoPagoID;

                    var p = new Pago();
                    p = new Pago();
                    p.PrestamoPagoID = id;
                    p.Cuota = pag.Cuota;
                    p.ValorPago = pag.ValorPago - pago.ValorPago;
                    p.Saldo = pag.Saldo;
                    p.FechaPago = pag.FechaPago;
                    p.Pagado = false;
                    context.Pago.Add(p);

                    Pago pg = context.Pago.FirstOrDefault(x => x.IDPago == pago.IDPago);
                    pg.Pagado = true;
                    pg.Saldo = pag.Saldo + pago.ValorPago;

                    context.SaveChanges();
                }
                else
                {

                    if (pago.ValorPago > pag.ValorPago)
                    {

                        var pagosFaltantes = (from p in context.Pago
                                              join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                                              where pp.NoPrestamo == presta.NoPrestamo && p.Pagado == false
                                              orderby p.Cuota
                                              select p).ToList();

                        var sumaPagos = pagosFaltantes.Sum(z => z.ValorPago);

                        if (pago.ValorPago > sumaPagos)
                        {
                            throw new Exception("El valor ingresado supera el saldo pendiente de pago.");
                        }
                        else
                        {
                            var splitPago = pago.ValorPago;

                            foreach (Pago r in pagosFaltantes)
                            {
                                if (splitPago > 0)
                                {
                                    splitPago = splitPago - r.ValorPago;

                                    if (splitPago < 0)
                                    {
                                        splitPago = splitPago + r.ValorPago;

                                        var pc = new PagoCuota();
                                        pc = new PagoCuota();
                                        pc.IDPago = r.IDPago;
                                        pc.Valor = splitPago;
                                        pc.FechaPago = pago.FechaPago;
                                        context.PagoCuota.Add(pc);

                                        var pp = new PrestamoPago();
                                        pp = new PrestamoPago();
                                        pp.NoPrestamo = presta.NoPrestamo;
                                        context.PrestamoPago.Add(pp);

                                        int id = pp.PrestamoPagoID;

                                        var p = new Pago();
                                        p = new Pago();
                                        p.PrestamoPagoID = id;
                                        p.Cuota = r.Cuota;
                                        p.ValorPago = r.ValorPago - splitPago;
                                        p.Saldo = r.Saldo;
                                        p.FechaPago = r.FechaPago;
                                        p.Pagado = false;
                                        context.Pago.Add(p);

                                        Pago pg = context.Pago.FirstOrDefault(x => x.IDPago == r.IDPago);
                                        pg.Pagado = true;
                                        pg.Saldo = r.Saldo + splitPago;

                                        context.SaveChanges();

                                        splitPago = splitPago - r.ValorPago;

                                    }
                                    else
                                    {
                                        var pc = new PagoCuota();
                                        pc = new PagoCuota();
                                        pc.IDPago = r.IDPago;
                                        pc.Valor = r.ValorPago;
                                        pc.FechaPago = pago.FechaPago;
                                        context.PagoCuota.Add(pc);

                                        Pago pg = context.Pago.FirstOrDefault(x => x.IDPago == r.IDPago);
                                        pg.Pagado = true;

                                        context.SaveChanges();

                                    }
                                  
                                }                           

                            }
                        }
                    }
                    else
                    {
                        var pc = new PagoCuota();
                        pc = new PagoCuota();
                        pc.IDPago = pago.IDPago;
                        pc.Valor = pago.ValorPago;
                        pc.FechaPago = DateTime.Now;
                        context.PagoCuota.Add(pc);

                        Pago pg = context.Pago.FirstOrDefault(x => x.IDPago == pago.IDPago);
                        pg.Pagado = true;

                        context.SaveChanges();
                    }

                }

                var a = new Abono();
                a = new Abono();
                a.IdPago = IDPagoAbono;
                a.Valor = Abono;
                a.Fecha = DateTime.Now;
                context.Abono.Add(a);
                context.SaveChanges();

                var saldoCuotas = (from p in context.Pago
                             join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                             join pc in context.PagoCuota on p.IDPago equals pc.IDPago
                             where pp.NoPrestamo == presta.NoPrestamo                              
                             select pc).ToList();

                var saldo = saldoCuotas.Sum(x => x.Valor);

                pres.Saldo = pres.Total - saldo;
                context.SaveChanges();

                var cancelaPrestamo = context.Prestamo.FirstOrDefault(x => x.NoPrestamo == presta.NoPrestamo);

                if (cancelaPrestamo.Saldo == 0)
                {
                    cancelaPrestamo.Estado = false;
                    context.SaveChanges();
                }

            }        

        }

        private int NumeroCuotaXNoPrestamo(int NoPrestamo)
        {
            int? query ;
            int id = 0;
            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         where pp.NoPrestamo == NoPrestamo                         
                         orderby p.Cuota descending
                         select p.Cuota).FirstOrDefault();
            }

            if (query == null) id = 1;
            else  id = int.Parse(query.ToString()) + 1;

            return id;
        }

        public List<Pago> GetPagosXPrestamoID(int prestamoID)
        {

            var query = new List<Pago>();

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         where pp.NoPrestamo == prestamoID 
                         orderby p.Cuota ascending
                         select p).ToList();
            }

            return query;
        }


        public List<PagosDTO> ConsultaPagos(int prestamoID)
        {

            var query = new List<PagosDTO>();

            using (var context = new PrestamosEntities())
            {                
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         from pc in context.PagoCuota.Where(x => x.IDPago == p.IDPago).DefaultIfEmpty()
                         where pp.NoPrestamo == prestamoID
                         orderby p.Cuota, p.IDPago ascending
                         select new PagosDTO
                         {
                             IDPago = p.IDPago,
                             PrestamoPagoID = pp.PrestamoPagoID,
                             Cuota = p.Cuota,
                             ValorPago = p.ValorPago,
                             Saldo = p.Saldo,
                             FechaPago = p.FechaPago,
                             Pagado = p.Pagado,
                             Valor = pc.Valor,
                             FechaPagoReal = pc.FechaPago
                         }).ToList();
            }

            return query;
        }

        public decimal TotalPagosxDia(DateTime fechaInicial, DateTime fechaFinal)
        {
            var query = new List<PagoCuota>();
            decimal pagos;

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago                         
                         join pc in context.PagoCuota on p.IDPago equals pc.IDPago                   
                         where pc.FechaPago >= fechaInicial && p.FechaPago <= fechaFinal
                         select pc).ToList();
            }

            pagos = query.Sum(x => x.Valor);

            return pagos;

        }

        public int NoAbonosXDia(DateTime fechaInicial, DateTime fechaFinal)
        {
            var query = new List<PagoCuota>();            

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pc in context.PagoCuota on p.IDPago equals pc.IDPago
                         where pc.FechaPago >= fechaInicial && p.FechaPago <= fechaFinal
                         select pc).ToList();
            }

            return query.Count();

        }

        public void EliminarPago(int idPago)
        {
            using (var context = new PrestamosEntities())
            {

                Pago pago = context.Pago.Where(x => x.IDPago == idPago).FirstOrDefault();
                pago.Pagado = false;

                PagoCuota pagoCuota = context.PagoCuota.FirstOrDefault(x => x.IDPago == idPago);
                context.PagoCuota.Remove(pagoCuota);

                context.SaveChanges();

            }

        }

        public Pago GetCuota(int idpago)
        {
            var query = new Pago();

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         where p.IDPago == idpago                         
                         select p).FirstOrDefault();
            }

            return query;
        }

        public decimal PorCobrar(DateTime fechaIni, DateTime FechaFinal)
        {
            decimal valor = 0;     
            using (var context = new PrestamosEntities())
            {
               

                var query = (from p in context.Pago
                             where p.Pagado == false && p.FechaPago >= fechaIni && p.FechaPago <= FechaFinal
                             select p).ToList();

                if (query == null) valor = 0;
                else valor = decimal.Parse(query.Sum(x => x.ValorPago).ToString());
            }
            

            return valor;
        }


        public List<Pago> CuotasXPagar(int prestamoID)
        {
            var query = new List<Pago>();

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         where pp.NoPrestamo == prestamoID && p.Pagado == false
                         orderby p.Cuota ascending
                         select p).ToList();
            }

            return query;
        }

        public List<PagosDTO> GetPagosXPrestamoIDPagados(int prestamoID)
        {

            var query = new List<PagosDTO>();

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         join pc in context.PagoCuota on p.IDPago equals pc.IDPago
                         where pp.NoPrestamo == prestamoID 
                         orderby p.Cuota, p.IDPago ascending
                         select new PagosDTO() {
                             IDPago = pc.IDPago,
                             Cuota = p.Cuota,                             
                             Valor = pc.Valor,
                             FechaPagoReal = pc.FechaPago,
                             Pagado = p.Pagado
                         }).ToList();
            }

            return query;
        }

        public List<PagoCuota> GetPagosCuotasXPrestamoID(int prestamoID)
        {

            var query = new List<PagoCuota>();

            using (var context = new PrestamosEntities())
            {
                query = (from p in context.Pago
                         join pp in context.PrestamoPago on p.PrestamoPagoID equals pp.PrestamoPagoID
                         join pc in context.PagoCuota on p.IDPago equals pc.IDPago
                         where pp.NoPrestamo == prestamoID                          
                         select pc).ToList();
            }

            return query;
        }

        public List<MorososDTO> Morosos()
        {
            var query = new List<MorososDTO>();
            using (var context = new PrestamosEntities())
            {

                 query = (from p in context.Prestamo
                          join c in context.Cliente on p.Documento equals c.Documento
                          join pp in context.PrestamoPago on p.NoPrestamo equals pp.NoPrestamo
                          join pg in context.Pago on pp.PrestamoPagoID equals pg.PrestamoPagoID
                          where pg.Pagado == false && pg.FechaPago <= DateTime.Now
                          select new MorososDTO()
                          {
                              Documento = c.Documento,
                              Nombre = c.Nombre,
                              NoPrestamo = p.NoPrestamo,
                              Cuota = pg.Cuota,
                              FechaPago = pg.FechaPago,
                              ValorCuota = pg.ValorPago
                          }
                          ).ToList();                
            }


            return query;
        }

        public List<Pago> RecaudosXFechaTotal(DateTime fechaDesde, DateTime fechaHasta)
        {
            var query = new List<Pago>();
            using (var context = new PrestamosEntities())
            {

                query = (from p in context.Prestamo                     
                         join pp in context.PrestamoPago on p.NoPrestamo equals pp.NoPrestamo
                         join pg in context.Pago on pp.PrestamoPagoID equals pg.PrestamoPagoID                        
                         select pg).ToList();
            }

            List<Pago> datos =  query.Where(x => x.FechaPago.Date >= fechaDesde.Date && x.FechaPago.Date <= fechaHasta.Date).ToList();

            return datos;
        }


        public List<PagosDTO> RecaudosXFechaDetalle(DateTime fechaDesde, DateTime fechaHasta)
        {
            var query = new List<PagosDTO>();
            using (var context = new PrestamosEntities())
            {

                query = (from p in context.Prestamo
                         join c in context.Cliente on p.Documento equals c.Documento
                         join pp in context.PrestamoPago on p.NoPrestamo equals pp.NoPrestamo
                         join pg in context.Pago on pp.PrestamoPagoID equals pg.PrestamoPagoID      
                         orderby c.Nombre, pg.Cuota                                           
                         select new PagosDTO() {
                             Nombre = c.Nombre,
                             Cuota = pg.Cuota,                             
                             ValorPago = pg.ValorPago,
                             FechaPago = pg.FechaPago,
                             Pagado = pg.Pagado
                             
                         }).ToList();
                
            }

            List<PagosDTO> datos = query.Where(x => x.FechaPago >= fechaDesde.Date && x.FechaPago <= fechaHasta.Date).ToList();

            return datos;
        }

        public Abono UltimoAbono(int NoPrestamo)
        {
            var query = new Abono();
            using (var context = new PrestamosEntities())
            {

                query = (from p in context.Prestamo
                         join pp in context.PrestamoPago on p.NoPrestamo equals pp.NoPrestamo
                         join pg in context.Pago on pp.PrestamoPagoID equals pg.PrestamoPagoID
                         join a in context.Abono  on pg.IDPago equals a.IdPago
                         where p.NoPrestamo == NoPrestamo
                         orderby   a.Fecha descending
                         select a).FirstOrDefault();
            }

            return query;
        }
    }


}
