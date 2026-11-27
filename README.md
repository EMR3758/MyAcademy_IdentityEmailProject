# 📧 Mailora – Modern E-posta Yönetim Sistemi

Mailora, ASP.NET Core MVC teknolojisi kullanılarak geliştirilmiş modern bir e-posta yönetim uygulamasıdır. Kullanıcıların e-postalarını tek bir platform üzerinden yönetmesini, mesajlarını düzenlemesini ve e-posta aktivitelerini takip etmesini sağlar.

Proje; güvenli kullanıcı kimlik doğrulama, e-posta doğrulama, mesaj yönetimi ve kullanıcı dostu bir arayüz sunmayı amaçlamaktadır.

## ✨ Özellikler

### 🔐 Kullanıcı ve Hesap Yönetimi
- Kullanıcı kayıt ve giriş işlemleri
- E-posta doğrulama kodu sistemi
- ASP.NET Core Identity ile kimlik doğrulama
- Kullanıcı profil yönetimi
- Profil fotoğrafı güncelleme

### 📩 E-posta Yönetimi
- Gelen kutusu yönetimi
- E-posta gönderme ve alma
- Gönderilen mesajları görüntüleme
- Taslak oluşturma ve düzenleme
- Mesajları favorilere ekleme
- Çöp kutusu yönetimi
- Silinen mesajları geri yükleme
- Mesajları kalıcı olarak silme
- Mesajları okundu veya okunmadı olarak işaretleme

### 📊 Dashboard ve Analiz
- Kullanıcıya özel kontrol paneli
- Gelen ve gönderilen mesaj istatistikleri
- Okunmamış mesaj takibi
- Favori ve taslak sayıları
- Günlük gelen ve gönderilen mesaj grafiği
- Kategori dağılım grafiği
- Son gelen ve gönderilen mesajlar
- Hızlı işlem bağlantıları

### 🎨 Kullanıcı Arayüzü
- Modern ve duyarlı tasarım
- Yan menü ile kolay gezinme
- Kullanıcı profil menüsü
- Bildirim alanı
- Arama arayüzü
- Mesaj yönetimi için özel ekranlar

## 🛠️ Kullanılan Teknolojiler

| Teknoloji | Kullanım Amacı |
|---|---|
| C# | Backend programlama dili |
| ASP.NET Core MVC (.NET 9) | Web uygulaması altyapısı |
| Entity Framework Core | ORM ve veritabanı işlemleri |
| Microsoft SQL Server | İlişkisel veritabanı |
| ASP.NET Core Identity | Kullanıcı ve kimlik yönetimi |
| Razor Views | Dinamik arayüz oluşturma |
| Bootstrap 5 | Duyarlı arayüz bileşenleri |
| HTML5 / CSS3 / JavaScript | Frontend geliştirme |
| MailKit | SMTP üzerinden e-posta gönderimi |

## 📸 Ekran Görüntüleri

### 🔐 Kullanıcı İşlemleri

**Kayıt Olma Ekranı**

<img width="1324" height="997" alt="Ekran görüntüsü 2026-11-27 155900" src="https://github.com/user-attachments/assets/367f3bee-cbd0-4994-a5bc-6f075d2c2c27" />

**E-posta Doğrulama Ekranı**

<img width="733" height="997" alt="Ekran görüntüsü 2026-11-27 162330" src="https://github.com/user-attachments/assets/76fb00ea-aad1-407b-bdc9-dee052c3be68" />

**Giriş Ekranı**

<img width="1172" height="864" alt="Ekran görüntüsü 2026-11-27 162348" src="https://github.com/user-attachments/assets/83088821-9f9c-49e4-9e0b-05fc7e07d94b" />

### 📊 Dashboard

<img width="1903" height="1033" alt="Ekran görüntüsü 2026-11-27 162434" src="https://github.com/user-attachments/assets/95a7539e-8ed4-4f8d-8fa7-0ef44aca70f1" />

Dashboard ekranında kullanıcıya ait e-posta istatistikleri, mesaj trafiği, kategori dağılımı ve son mesajlar görüntülenmektedir.

### 📩 E-posta Yönetimi

**Gelen Kutusu**

<img width="1904" height="1031" alt="Ekran görüntüsü 2026-11-27 162449" src="https://github.com/user-attachments/assets/4dfe06af-533b-4a2b-a5be-f2d891b85964" />


**Gönderilen Mesajlar**

<img width="1904" height="1028" alt="Ekran görüntüsü 2026-11-27 162457" src="https://github.com/user-attachments/assets/010bdb69-4b80-4fd4-81c7-c27f97225b8f" />


**Taslaklar**

<img width="1911" height="1025" alt="Ekran görüntüsü 2026-11-27 162505" src="https://github.com/user-attachments/assets/46913533-8b07-4773-bf03-efaa1ac4e88f" />

**Yıldızlı Mesajlar**

<img width="1904" height="1036" alt="Ekran görüntüsü 2026-11-27 162513" src="https://github.com/user-attachments/assets/83a096cd-65ce-4e19-8e3d-e00f0ea781aa" />

**Çöp Kutusu**

<img width="1907" height="1027" alt="Ekran görüntüsü 2026-11-27 162551" src="https://github.com/user-attachments/assets/53d1ee3a-3739-4809-9423-fda8d000839a" />

<img width="1908" height="1028" alt="Ekran görüntüsü 2026-11-27 162530" src="https://github.com/user-attachments/assets/376c1f5b-f278-4547-bd56-326fac9c6f5c" />

### 👤 Profil Yönetimi

<img width="1913" height="1042" alt="Ekran görüntüsü 2026-11-27 162606" src="https://github.com/user-attachments/assets/ae0f5b3b-8cd1-4910-96fd-c1c4c8905f5f" />


## 🏗️ Proje Mimarisi

Mailora, ASP.NET Core MVC mimarisi temel alınarak geliştirilmiştir.

- **Controllers:** HTTP isteklerini karşılar ve uygulama akışını yönetir.
- **Models:** ViewModel ve uygulama içerisinde kullanılan veri modellerini barındırır.
- **Entities:** Veritabanı tablolarını temsil eden sınıfları içerir.
- **Views:** Razor kullanılarak oluşturulan kullanıcı arayüzlerini barındırır.
- **Context:** Entity Framework Core üzerinden veritabanı bağlantısını ve işlemlerini yönetir.
- **Services:** E-posta gönderimi gibi tekrar kullanılabilir uygulama işlemlerini yürütür.

## 🔒 Güvenlik

- ASP.NET Core Identity ile kimlik doğrulama ve yetkilendirme
- Kayıt sırasında e-posta doğrulama
- Yetkisiz erişime karşı korunan sayfalar
- Kullanıcıya özel mesaj erişim kontrolleri
- Form işlemlerinde CSRF koruması
- SMTP bilgilerinin User Secrets ile güvenli şekilde saklanması

## ⚙️ Kurulum

### Gereksinimler
- .NET 9 SDK
- Microsoft SQL Server
- Visual Studio 2022 veya uyumlu bir IDE

### Projeyi Çalıştırma

1. Projeyi bilgisayarınıza klonlayın:

   ```bash
   git clone REPOSITORY_URL
   ```

2. Proje klasörüne gidin:

   ```bash
   cd Mailora
   ```

3. Veritabanı bağlantı ayarlarını yapılandırın.

4. SMTP ayarlarını User Secrets üzerinden tanımlayın.

5. Veritabanı migration işlemlerini uygulayın:

   ```bash
   dotnet ef database update
   ```

6. Uygulamayı çalıştırın:

   ```bash
   dotnet run
   ```

## 🚀 Geliştirme Planı

- Gerçek zamanlı bildirim sistemi
- E-posta eki yönetimi
- Gelişmiş arama ve filtreleme
- Geliştirilmiş e-posta kategorilendirme
- Dashboard analizlerinin genişletilmesi

## 👨‍💻 Geliştirici

**Emirhan Özcan**

Bilgisayar Mühendisi

---

*Mailora – E-posta yönetiminde modern ve düzenli bir deneyim.*
