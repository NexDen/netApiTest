using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DersDb>(opt => opt.UseNpgsql("Host=localhost; Database=postgres; Username=postgres; Password=1234"));
var app = builder.Build();
/*
    DB refeansı:
        DB adı: postgres
        DB kullanıcı adı: postgres
        DB şifresi: 1234
    Örnek 100 tane ders objesi "./örnek veriler.txt" dosyasında yer almaktadır.
    
    Api link referansı:
 
    GET - api/dersler?{id: bağımlı}&{ad: bağımlı} => dersler listesinden id ve/veya ad ile uyuşan dersi verir,
                                                         eğer ad veya id verilmemişse kabul etmez.
                                                         id ve/veya ad değişkeni verilmesi zorunludur.
            
    GET - api/dersler/page?{pageSize: opsiyonel=20}&{pageNumber: opsiyonel=1} => dersler listesinin belirli bir kısmını
                                                                            pageSize ve pageNumber değişkenlerine
                                                                            bağlı olarak verir.
                                    
    POST - api/dersler/ (body: Ders) => ders listesine ders ekler, eğer verilen dersin id'sine eşit bir ders varsa hata verir.
    
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
        var ders = await db.dersler.SingleOrDefaultAsync(x => x.id == id && x.ad == ad);
        return ders != null ? Results.Ok(ders) : Results.NotFound();
    }
    if (id != -1){
        var ders = await db.dersler.SingleOrDefaultAsync(x => x.id == id);
        return ders != null ? Results.Ok(ders) : Results.NotFound(); // sadece bir id verilmiş ise
    }
    if (ad.Length > 0){
        var ders = await db.dersler.SingleOrDefaultAsync(x => x.ad == ad);
        return ders != null ? Results.Ok(ders) : Results.NotFound(); // sadece bir id verilmiş ise
    }
    return Results.BadRequest("Ad veya ID parametresi verilmelidir.");
});

app.MapGet("/api/dersler/page", async (DersDb db, int pageNumber = 1, int pageSize = 20) =>
{
    var dersler = await db.dersler.ToArrayAsync();
    return dersler.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    //                       ^ başlangıç indexi ^        
});

app.MapPost("api/dersler/", async (Ders inputDers, DersDb db) =>
{
    var ders = db.dersler.FirstOrDefault(x => x.id == inputDers.id);
    if (ders != null) return Results.Conflict(ders.id + " " + "ID'sinde bir ders mevcut."); // önceden tanımlanmış id ile bir ders varsa reddet
    db.dersler.Add(inputDers);
    await db.SaveChangesAsync();
    return Results.Created($"dersler/{inputDers.id}", inputDers);
});
app.MapPut("/api/dersler/", async (int id, Ders inputDers, DersDb db) =>
{
    var ders = await db.dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    
    ders.ad = inputDers.ad ?? ders.ad;
    ders.saatler = inputDers.saatler ?? ders.saatler;
    
    await db.SaveChangesAsync();
    return Results.Ok(ders);
});
app.MapPatch("/api/dersler/", async (int id, Ders inputDers, DersDb db) =>
{
    var ders = await db.dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    
    ders.ad = inputDers.ad ?? ders.ad;
    ders.saatler = inputDers.saatler ?? ders.saatler;
    
    await db.SaveChangesAsync();
    return Results.Ok(ders);
});
app.MapDelete("/api/dersler/", async (int id, DersDb db) =>
{
    var ders = await db.dersler.FindAsync(id);
    if (ders == null) return Results.NotFound();
    db.dersler.Remove(ders);
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.Run();