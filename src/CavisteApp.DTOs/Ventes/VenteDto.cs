using System;
using System.Collections.Generic;
using System.Text;

namespace CavisteApp.DTOs.Ventes;

public class VenteDto
{
    public int Id { get; set; }
    public DateTime Date{ get; set; }
    public decimal MontantTotal { get; set; }
}
