# 🚀 БЫСТРЫЙ СТАРТ-ГАЙД

## Для тех, кто хочет быстро запустить и понять проект

---

## ⚡ ЗАПУСК (5 минут)

### 1. Требования
- ✅ Visual Studio 2019+ (или VS Community)
- ✅ .NET Framework 4.8
- ✅ SQLite (встроен в проект)

### 2. Запуск
```bash
# Откройте solution
Admissions_Reserve.sln

# Нажмите F5 или Ctrl+F5
# Готово! ✅
```

### 3. Первый запуск
При первом запуске приложение автоматически:
- ✅ Создаст БД в `App_Data\AdmissionsReserve.db`
- ✅ Инициализирует таблицы
- ✅ Покажет путь к БД

---

## 🎯 ОСНОВНЫЕ ФУНКЦИИ

### 📝 Заполнение заявления (10 этапов)

**Каждый этап:**
1. Введите требуемые данные
2. Нажмите "Далее" (Next)
3. Данные автоматически сохраняются
4. Переход на следующий этап

**Этапы:**
```
1. Удостоверение личности
   ├─ ФИО, дата рождения
   ├─ Тип и реквизиты документа
   └─ Адреса регистрации и проживания

2. Контактная информация
   ├─ Телефоны, email
   ├─ Адрес проживания
   └─ Соцсети и предпочтения

3. Образование
   ├─ Тип заявления
   ├─ Документ об образовании
   └─ Оценки и информация

4. Документы
   ├─ Документы, удостоверяющие личность
   └─ Прочие документы

5. Родственники
   ├─ Информация о родственниках
   └─ Контакты

6. Дополнительно
   ├─ Языки и уровни владения
   ├─ Спортивные достижения
   └─ Военная служба

7. Конкурсы
   ├─ Выбор доступных конкурсов
   └─ Установка приоритетов

8. Достижения
   ├─ Олимпиады, награды
   ├─ Значок ГТО
   └─ Загрузка документов

9. Приоритеты
   ├─ Установка порядка предпочтений
   └─ Финальное подтверждение

10. Приложенные документы
    ├─ Образование (PDF)
    ├─ Документы (копии)
    └─ Завершение заявления
```

---

## 📋 ВАЛИДАЦИЯ ДАННЫХ

### ✅ Проверяется автоматически:

**IdentityPage:**
- ✓ ФИ (обязательны)
- ✓ Возраст ≥ 16 лет
- ✓ СНИЛС (если указан): 11 цифр
- ✓ Даты не в будущем

**ContactsPage:**
- ✓ Email (обязателен и валиден)
- ✓ Минимум один телефон
- ✓ Населенный пункт

**DocumentsPage:**
- ✓ Номер документа
- ✓ Серия и номер содержат цифры

**AdditionalInfoPage:**
- ✓ СНИЛС (11 цифр)
- ✓ ИНН (10 или 12 цифр)

**ApplicationTypeAndEducationPage:**
- ✓ Оценки неотрицательные
- ✓ Минимум одна оценка

---

## 🗂️ СТРУКТУРА ПРОЕКТА

```
Admissions_Reserve/
├─ View/
│  ├─ IdentityPage.xaml
│  ├─ ContactsPage.xaml
│  ├─ DocumentsPage.xaml
│  ├─ RelativesPage.xaml
│  ├─ AdditionalInfoPage.xaml
│  ├─ ApplicationTypeAndEducationPage.xaml
│  ├─ ApplicationCompetitionsPage.xaml
│  ├─ IndividualAchievementsPage.xaml
│  ├─ PrioritiesPage.xaml
│  ├─ AttachedDocumentsPage.xaml
│  └─ Converters.cs (вспомогательные классы)
│
├─ Model/
│  ├─ Models.cs (сущности БД)
│  ├─ DataService.cs (работа с БД)
│  ├─ ValidationHelper.cs (валидация)
│  ├─ SessionManager.cs (текущая сессия)
│  └─ DatabaseHelper.cs (подключение БД)
│
├─ MainWindow.xaml (главное окно)
├─ App.xaml (инициализация)
└─ Admissions_Reserve.sln (решение)
```

---

## 🔧 ДОБАВЛЕНИЕ НОВОГО ПОЛЯ

### Пример: Добавить поле "Паспортные данные"

#### Шаг 1: Добавить в БД
```csharp
// В DatabaseHelper.cs добавить в CREATE TABLE Applicants:
PassportSeries TEXT,
PassportNumber TEXT,
PassportIssueDate DATETIME
```

#### Шаг 2: Добавить в Model
```csharp
// В Models.cs, класс Applicants:
public string PassportSeries { get; set; }
public string PassportNumber { get; set; }
public DateTime? PassportIssueDate { get; set; }
```

#### Шаг 3: Добавить в XAML
```xaml
<!-- В IdentityPage.xaml -->
<TextBlock Text="Серия паспорта" />
<TextBox x:Name="PassportSeriesTextBox" />

<TextBlock Text="Номер паспорта" />
<TextBox x:Name="PassportNumberTextBox" />
```

#### Шаг 4: Добавить валидацию и сохранение
```csharp
// В IdentityPage.xaml.cs, метод SaveData():
currentApplicant.PassportSeries = PassportSeriesTextBox.Text?.Trim();
currentApplicant.PassportNumber = PassportNumberTextBox.Text?.Trim();
currentApplicant.PassportIssueDate = PassportIssueDatePicker.SelectedDate;

// В методе ValidateData():
if (string.IsNullOrWhiteSpace(PassportNumberTextBox.Text))
    errors.Add("• Номер паспорта обязателен");
```

#### Шаг 5: Запуск
- ✅ Компилируем
- ✅ Запускаем
- ✅ БД создается с новыми полями
- ✅ Готово!

---

## 🐛 ЧАСТЫЕ ОШИБКИ И РЕШЕНИЯ

### ❌ "Абитуриент не создан" при открытии страницы

**Причина:** Вы попытались открыть страницу без заполнения предыдущих  
**Решение:** Начните с IdentityPage и заполняйте последовательно

### ❌ "Локальная переменная 'series' уже объявлена"

**Причина:** Одно имя переменной используется дважды в разных областях  
**Решение:** Переименуйте одну из переменных (используйте префикс document_)

### ❌ БД не создается при запуске

**Причина:** Нет прав на запись в папку `App_Data`  
**Решение:** 
- Запустите VS как администратор
- Или создайте папку `App_Data` вручную

### ❌ "Значение null" при сохранении

**Причина:** Не все обязательные поля заполнены  
**Решение:** Посмотрите сообщение об ошибке валидации, заполните поле

---

## 📊 РАБОТА С ДАННЫМИ

### Просмотр БД

**Вариант 1: Через VS**
- Server Explorer → Data Connections → AdmissionsReserve.db

**Вариант 2: SQLite Browser**
- Скачайте DB Browser for SQLite
- Откройте файл `App_Data\AdmissionsReserve.db`

### Таблицы БД

```sql
-- Основные таблицы:
Applicants              -- Абитуриенты
IdentityDocuments       -- Удостоверения личности
Documents              -- Документы
Relatives              -- Родственники
EducationDocuments     -- Образование
IndividualAchievements -- Достижения
SportAchievements      -- Спортивные достижения
ApplicantLanguages     -- Языки
ApplicationCompetitions -- Конкурсы
ApplicationPriorities   -- Приоритеты

-- Справочные таблицы:
IdentityDocumentTypes  -- Типы удостоверений
DocumentTypes          -- Типы документов
Countries             -- Страны
Genders               -- Пол
Citizenships          -- Гражданство
```

---

## ✅ ЧЕК-ЛИСТ ДЛЯ ТЕСТИРОВАНИЯ

- [ ] Запуск приложения без ошибок
- [ ] Заполнение всех 10 этапов
- [ ] Валидация срабатывает для пустых полей
- [ ] Данные сохраняются в БД
- [ ] Можно вернуться на предыдущий этап
- [ ] Нельзя пропустить обязательный этап
- [ ] Финальное сообщение об успехе
- [ ] БД создана в App_Data

---

## 🎓 ОБУЧЕНИЕ

### Хотите изучить проект?

1. **Начните с:**
   - MainWindow.xaml - главное окно
   - SessionManager.cs - управление сессией

2. **Потом изучите:**
   - IdentityPage - простейшая страница
   - DataService.cs - работа с БД

3. **Продвинутые темы:**
   - ValidationHelper - валидация
   - DatabaseHelper - инициализация БД
   - Конвертеры в XAML

---

## 🆘 ПОЛУЧИТЬ ПОМОЩЬ

### Если что-то не работает:

1. **Проверьте Output окно** (View → Output)
2. **Посмотрите Exception** (Debug → Windows → Exception Settings)
3. **Добавьте Breakpoint** (F9) и выполняйте пошагово (F10)
4. **Читайте документацию:**
   - AUDIT_REPORT.md - что было исправлено
   - DEVELOPMENT_GUIDE.md - подробное руководство

---

## 🚀 БЫСТРЫЕ КОМАНДЫ

```csharp
// Получить текущего абитуриента
var applicant = SessionManager.CurrentApplicant;

// Сохранить абитуриента
DataService.UpdateApplicant(applicant);

// Загрузить все документы
var docs = DataService.GetApplicantDocuments(applicantId);

// Валидация email
bool valid = ValidationHelper.IsValidEmail("test@example.com");

// Валидация СНИЛС
bool valid = ValidationHelper.IsValidSnils("123-45-678-90");

// Логирование (если добавлено)
LogHelper.Log("Сообщение", exception);
```

---

## 📞 КОНТАКТЫ РАЗРАБОТЧИКА

GitHub: https://github.com/ViFiL-black/AdmisReserv

---

**Версия:** 1.0  
**Дата:** 2024  
**Статус:** ✅ ГОТОВО К ИСПОЛЬЗОВАНИЮ

## 💡 Совет

Начните с заполнения заявления от начала до конца - это лучший способ понять, как работает приложение! 🎉
