using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DersDb>(opt => opt.UseInMemoryDatabase("DersListesi"));
var app = builder.Build();
/*
    Api link referansı:
 
    GET - api/dersler?{id: opsiyonel}&{ad: opsiyonel} => dersler listesinden id ve/veya ad ile uyuşan dersi verir,
                                                         eğer ad veya id verilmemişse tüm ders listesini verir.
                                                         
    POST - api/api/dersler/ (body: Ders) => ders listesine ders ekler, eğer verilen dersin id'sine eşit bir ders varsa hata verir.
    
    DELETE - api/dersler?{id: gerekli} => dersler listesinden belirtilen id'ye sahip olan dersi siler.
    
    PATCH/PUT - api/dersler?{id: gerekli} (body: Ders*) => dersler listesindeki belirtilen id'ye sahip olan dersin 
                                                          verilen bilgilerini günceller.
                                                          *: Bütün ders özelliklerini barındırması gerekmez.

    Örnek request body:
    {
        "id" : 41,
        "ad" : "CMPE210",
        "saatler" : "TT56" (Salı, Salı, 5. Ders Saati, 6. Ders Saati)
    }

    JetBrains Rider ile yaptım, Visual Studio'da test edecek zamanım olmadı.
*/

app.MapGet("/api/dersler/", async (DersDb db, int id = -1, string ad = "") =>
{
    if (id != -1 && ad.Length > 0) // eğer hem ad hem de bir id verilmiş ise
    {
        var ders = await db.Dersler.SingleOrDefaultAsync(x => x.Id == id && x.Ad == ad);
        return ders != null ? Results.Ok(ders) : Results.NotFound();
    }
    if (id != -1) return Results.Ok(db.Dersler.FirstOrDefault(x => x.Id == id)); // sadece bir id verilmiş ise
    if (ad.Length > 0) return Results.Ok(db.Dersler.FirstOrDefault(x => x.Ad == ad)); // sadece bir isim verilmiş ise
    return Results.Ok(await db.Dersler.ToListAsync()); // hiç bir şey verilmemiş ise
});
app.MapPost("api/dersler/", async (Ders inputDers, DersDb db) =>
{
    var ders = db.Dersler.FirstOrDefault(x => x.Id == inputDers.Id);
    if (ders != null) return Results.Conflict(ders.Id + " " + "ID'sinde bir ders mevcut."); // önceden tanımlanmış id ile bir ders varsa reddet
    db.Dersler.Add(inputDers);
    await db.SaveChangesAsync();
    return Results.Created($"dersler/{inputDers.Id}", inputDers);
});
app.MapPut("/api/dersler/", async (int id, Ders inputDers, DersDb db) =>
{
    var ders = await db.Dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    
    ders.Ad = inputDers.Ad ?? ders.Ad;
    ders.Saatler = inputDers.Saatler ?? ders.Saatler;
    
    await db.SaveChangesAsync();
    return Results.Ok(ders);
});
app.MapPatch("/api/dersler/", async (int id, Ders inputDers, DersDb db) =>
{
    var ders = await db.Dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    
    ders.Ad = inputDers.Ad ?? ders.Ad;
    ders.Saatler = inputDers.Saatler ?? ders.Saatler;
    
    await db.SaveChangesAsync();
    return Results.Ok(ders);
});
app.MapDelete("/api/dersler/", async (int id, DersDb db) =>
{
    var ders = await db.Dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    db.Dersler.Remove(ders);
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.Run();