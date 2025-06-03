🚀 IndustryX – Production & E-Commerce Management System

IndustryX, üretim süreçlerini, hammadde tüketimini, stok yönetimini ve e-ticaret operasyonlarını tek bir platformda buluşturan kurumsal bir yönetim sistemidir. ASP.NET Core MVC (.NET 8) ile geliştirilmiş, modüler ve ölçeklenebilir yapıya sahiptir.

📦 İçerik
- Özellikler
- Kurulum
- Kullanım
- Teknolojiler
- İlgili Uygulamalar
- Ekran Görüntüleri
- Katkı Sağla
- Lisans

✨ Özellikler
- Ürün, hammadde ve depo bazlı stok yönetimi
- Reçete tanımlamaları ile üretim planlama
- Üretim ve maliyet analizleri
- Çoklu depo, üretim ve satış birimi desteği
- Rol bazlı kullanıcı yetkilendirme (Admin, Depocu, Şoför, Müşteri vb.)
- E-ticaret modülü: Sepet, Sipariş, Kargo Takibi, Favoriler
- Kritik stok uyarıları ve otomatik mail bildirimi
- Temiz mimari (Onion Architecture) & katmanlı yapı

⚙️ Kurulum

Gerekli Araçlar:
- .NET 8 SDK
- Visual Studio 2022+
- SQL Server
- (Opsiyonel) MongoDB

Adımlar:
1. Reponun klonlanması:
   git clone https://github.com/efekankzr/IndustryX.git
   cd IndustryX
   dotnet restore

2. appsettings.json içinde bağlantı ayarı:
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=IndustryXDb;Trusted_Connection=True;"
   }

3. Veritabanı oluşturma:
   dotnet ef database update

4. Projeyi çalıştırma:
   dotnet run

İlk kayıtlı admin hesabı:
Email: admin@industryx.com
Şifre: Admin123*

▶️ Kullanım
- Admin Panel: ürün, depo, üretim gibi işlemleri yürütün.
- Müşteri tarafı: alışveriş, sipariş takibi vb.
- Şoför & Depocu: görev bazlı arayüz desteği.

🧰 Kullanılan Teknolojiler
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- MSSQL
- Identity & Role-based Authorization
- Bootstrap 5
- jQuery, DataTables, SweetAlert2
- Onion Architecture
- AutoMapper
- MailKit

📱 İlgili Uygulamalar
- Mobil Uygulama – IndustryX Mobile (Android Studio & Java)
  GitHub: https://github.com/efekankzr/IndustryX-Mobile

- IoT Araç Takip Sistemi – ESP32 & SIM808
  GitHub: https://github.com/efekankzr/VehicleTracker

🤝 Katkı Sağla
1. Fork'la
2. Branch oluştur (git checkout -b feature/yenilik)
3. Commit yap (git commit -m 'Yeni özellik eklendi')
4. Push et (git push origin feature/yenilik)
5. Pull request gönder

📄 Lisans
Bu proje MIT lisansı ile lisanslanmıştır. Ayrıntılar için LICENSE dosyasını inceleyin.
