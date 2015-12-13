using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Helpers
{
    /// <summary>
    /// Класс для преобразования фамилии, имени и отчества (ФИО), наименования должности или подразделения, 
    /// заданных в именительном падеже в форму любого другого падежа, а также для восстановления 
    /// именительного падежа для ФИО, записанного в произвольном падеже. Склонение ФИО выполняется по правилам 
    /// склонения имен собственных, принятых в русском языке. ФИО для склонения может быть задано одной или 
    /// тремя строками при склонении и одной строкой – при восстановлении именительного падежа. Наименование 
    /// должности или подразделения задаются одной строкой.
    /// </summary>
    public static class Declension
    {
        #region Public properties and functions

        private static int m_MaxResultStringBufSize = 500;
        /// <summary>
        /// Размер внутреннего буффера в символах для возвращаемых сторк. По-умолчанию 500.
        /// </summary>
        public static int MaxResultBufSize
        {
            get
            {
                return m_MaxResultStringBufSize;
            }
            set
            {
                if(value < 1) 
                    throw new ArgumentOutOfRangeException("MaxResultBufSize", 
                        "Размер внутреннего буфера не может быть меньше 1");
                m_MaxResultStringBufSize = value;
            }
        }

        #region Declension functions

        /// <summary>
        /// Данная функция является основной в библиотеке и наиболее универсальной. 
        /// В качестве параметров ей необходимо передать ФИО в виде трех строк (каждая из которых может 
        /// быть пустой), явно указанный род, требуемое значение падежа. 
        /// При таких условиях этой функцией можно склонять ФИО и его составляющие в любых комбинациях. 
        /// Корректно обрабатываются фамилии с инициалами (Сидоров И.П.) – склоняться будет только фамилия 
        /// (у Сидорова И.П.). Допускается использование инициалов, состоящих более чем из одного символа 
        /// (Иванов Вс.Никод.). Кроме ФИО славянского типа эта функция может выполнять склонение корейских, 
        /// китайских и им подобным имен. При этом первое слово в таком имени соответствует фамилии, 
        /// второе – имени и третье – отчеству в наших терминах. Другими словами, при склонении Иванов Иван 
        /// Иванович и Ли Си Цын не требуется перестановка составляющих ФИО. Поскольку имена подобного вида 
        /// иногда записывают двумя словами (Ли Сицын), то при вызове функции склонения для такой формы записи 
        /// необходимо первым параметром передать пустую строку. В подавляющем большинстве случаев эта функция 
        /// пригодна и для склонения ФИО, записанного в формате "Фамилия Имя [Имя]" (Кеннеди Джон [Фиджеральд]). 
        /// Допускается использование признаков рода в фамилии (-оглы/-кызы), записанных через дефис. 
        /// </summary>
        /// <param name="surname">Фамилия</param>
        /// <param name="name">Имя</param>
        /// <param name="patronimic">Отчество</param>
        /// <param name="gender">Пол</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetSNPDeclension(string surname, string name, string patronimic, Gender gender,
            DeclensionCase declensionCase)
        {
            if(surname == null) throw new ArgumentNullException("surname");
            if(name == null) throw new ArgumentNullException("name");
            if(patronimic == null) throw new ArgumentNullException("patronimic");

            CheckGender(gender);
            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(surname, name, patronimic);

                int resultLen = MaxResultBufSize;
                int err = decGetFIOPadeg(ptrs[0], ptrs[1], ptrs[2], (Int32)gender, (Int32)declensionCase,
                    ptrs[3], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция предназначена для склонения ФИО, род которых неизвестен. Определение рода осуществляется 
        /// по окончанию отчества. Корректно обрабатываются отчества, имеющие признак рода: Оглы (сын) или 
        /// Кызы (дочь). Признак рода может записываться через дефис (Аскер-Оглы) или пробел (Аскер Оглы). 
        /// </summary>
        /// <param name="surname">Фамилия</param>
        /// <param name="name">Имя</param>
        /// <param name="patronimic">Отчество</param>
        /// <param name="gender">Пол</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetSNPDeclension(string surname, string name, string patronimic,
            DeclensionCase declensionCase)
        {
            if(surname == null) throw new ArgumentNullException("surname");
            if(name == null) throw new ArgumentNullException("name");
            if(patronimic == null) throw new ArgumentNullException("patronimic");

            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(surname, name, patronimic);

                int resultLen = MaxResultBufSize;
                int err = decGetFIOPadegAS(ptrs[0], ptrs[1], ptrs[2], (Int32)declensionCase,
                    ptrs[3], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция выполняет преобразование ФИО, заданного одной строкой и требует явного указания рода. 
        /// Порядок следования составляющих ФИО в строке параметра – фамилия, имя, отчество. Эта функция, 
        /// как и GetSNPDeclension, тоже допускает использование инициалов и может выполнять преобразование имен 
        /// типа китайских. Для корректной работы функции необходимо наличие трех компонент ФИО 
        /// (имена китайского типа допускается задавать двумя словами). В ряде случаев правильно обрабатываются 
        /// ФИО, записанные в формате "Фамилия Имя [Имя]".
        /// </summary>
        /// <param name="surnameNamePatronimic">ФИО0</param>
        /// <param name="gender">Пол</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetSNPDeclension(string surnameNamePatronimic, Gender gender,
            DeclensionCase declensionCase)
        {
            if(surnameNamePatronimic == null) throw new ArgumentNullException("surnameNamePatronimic");

            CheckGender(gender);
            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(surnameNamePatronimic);

                int resultLen = MaxResultBufSize;
                int err = decGetFIOPadegFS(ptrs[0], (Int32)gender, (Int32)declensionCase,
                    ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция предназначена для склонения ФИО, заданных одной строкой, род которых неизвестен. 
        /// Определение рода осуществляется по окончанию отчества. Функция корректно обрабатывает отчества, 
        /// имеющие признак рода: Оглы (сын) или Кызы (дочь). Признак рода может записываться через дефис 
        /// (Аскер-Оглы) или пробел (Аскер Оглы).
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="surname">Фамилия</param>
        /// <param name="gender">Пол</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetNSDeclension(string name, string surname, Gender gender,
            DeclensionCase declensionCase)
        {
            if(name == null) throw new ArgumentNullException("name");
            if(surname == null) throw new ArgumentNullException("surname");

            CheckGender(gender);
            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(name, surname);

                int resultLen = MaxResultBufSize;
                int err = decGetIFPadeg(ptrs[0], ptrs[1], (Int32)gender, (Int32)declensionCase,
                    ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция предназначена для склонения пар "Имя Фамилия" (Марк Твен) и требует явного указания рода. 
        /// Эта функция также пригодна для склонения имен собственных типа Джон Фиджеральд Кеннеди. 
        /// В этом случае Джон Фиджеральд следует передавать одним параметром, как имя. Разделитель слов в 
        /// параметре – пробел.
        /// </summary>
        /// <param name="nameSurname">ИФ</param>
        /// <param name="gender">Пол</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetNSDeclension(string nameSurname, Gender gender, DeclensionCase declensionCase)
        {
            if(nameSurname == null) throw new ArgumentNullException("nameSurname");

            CheckGender(gender);
            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(nameSurname);

                int resultLen = MaxResultBufSize;
                int err = decGetIFPadegFS(ptrs[0], (Int32)gender, (Int32)declensionCase,
                    ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция выполняет восстановление именительного падежа для ФИО, заданного в произвольном падеже в 
        /// формате "Фамилия Имя Отчество".
        /// </summary>
        /// <param name="surnameNamePatronimic">ФИО</param>
        /// <returns>ФИО в именительном падеже</returns>
        public static string GetNominativeDeclension(string surnameNamePatronimic)
        {
            if(surnameNamePatronimic == null) throw new ArgumentNullException("surnameNamePatronimic");

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(surnameNamePatronimic);

                int resultLen = MaxResultBufSize;
                int err = decGetNominativePadeg(ptrs[0], ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция предназначена для склонения наименования должностей, записанных одной строкой. 
        /// Начиная с версии библиотеки 3.3.0.21 стала возможной обработка составных должностей. 
        /// Разделителем в этом случае должна быть цепочка символов: пробел, дефис, пробел (‘ - ’). 
        /// При этом, каждая из должностей в в своем составе может содержать дефис (инженер-конструктор).
        /// </summary>
        /// <param name="appointment">Название должности</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetAppointmentDeclension(string appointment, DeclensionCase declensionCase)
        {
            if(appointment == null) throw new ArgumentNullException("appointment");

            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(appointment);

                int resultLen = MaxResultBufSize;
                int err = decGetAppointmentPadeg(ptrs[0], (Int32)declensionCase, ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция позволяет получить полное наименование должности и выполнить его преобразование в заданный 
        /// падеж. При объединении удаляются повторяющиеся слова при их наличии. Например: должность – 
        /// Начальник цеха; подразделение – Цех нестандартного оборудования; результат – Начальник цеха 
        /// нестандартного оборудования. 
        /// </summary>
        /// <param name="appointment">Название должности</param>
        /// <param name="office">Название подразделения</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetAppointmentOfficeDeclension(string appointment, string office,
            DeclensionCase declensionCase)
        {
            if(appointment == null) throw new ArgumentNullException("appointment");
            if(office == null) throw new ArgumentNullException("office");

            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(appointment, office);

                int resultLen = MaxResultBufSize;
                int err = decGetFullAppointmentPadeg(ptrs[0], ptrs[1], (Int32)declensionCase, ptrs[2], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        /// <summary>
        /// Функция предназначена для склонения наименования подразделений, записанных одной строкой. 
        /// Кроме подразделений функция также может выполнять склонение и наименований предприятий.
        /// </summary>
        /// <param name="office">Название подразделения</param>
        /// <param name="declensionCase">Падеж</param>
        /// <returns>Результат склонения</returns>
        public static string GetOfficeDeclension(string office, DeclensionCase declensionCase)
        {
            if(office == null) throw new ArgumentNullException("office");

            CheckDeclensionCase(declensionCase);

            IntPtr[] ptrs = null;
            try
            {
                ptrs = StringsToIntPtrArray(office);

                int resultLen = MaxResultBufSize;
                int err = decGetOfficePadeg(ptrs[0], (Int32)declensionCase, ptrs[1], ref resultLen);
                ThrowException(err);
                return IntPtrToString(ptrs, resultLen);
            }
            finally
            {
                FreeIntPtr(ptrs);
            }
        }

        #endregion Declension functions

        #region Service functions

        /// <summary>
        /// Позволяет определить род ФИО. Допускается параметром передавать не только отчество, но и ФИО 
        /// целиком. Главное, чтобы в передаваемой строке последним было отчество.
        /// </summary>
        /// <param name="patronimic">Отчество</param>
        /// <returns>Род</returns>
        public static Gender GetGender(string patronimic)
        {
            if(patronimic == null) throw new ArgumentNullException("patronimic");

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = StringToIntPtr(patronimic);

                return (Gender)decGetSex(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Позволяет определить падеж ФИО. К сожалению, однозначно определить падеж можно только учитывая 
        /// контекст предложения, поскольку запись, допустим, родительного и винительного падежей для мужских 
        /// ФИО совпадают. То же самое справедливо в отношении женских ФИО для дательного и предложного падежей. 
        /// В таких случаях функция возвращает значение Rodit для мужчин и Datel для женщин. 
        /// </summary>
        /// <param name="surnameNamePatronimic">ФИО</param>
        /// <returns>Падеж</returns>
        public static DeclensionCase GetDeclensionCase(string surnameNamePatronimic)
        {
            if(surnameNamePatronimic == null) throw new ArgumentNullException("surnameNamePatronimic");

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = StringToIntPtr(surnameNamePatronimic);

                return (DeclensionCase)decGetSex(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Позволяет выделить из заданного ФИО его части. Корректно обрабатываются ФИО, содержащие 
        /// признак рода (Оглы, Кызы) записанный через дефис или пробел
        /// </summary>
        /// <param name="surnameNamePatronimic">ФИО</param>
        /// <param name="surname">Фамилия</param>
        /// <param name="name">Имя</param>
        /// <param name="patronimic">Отчество</param>
        public static void GetSNM(string surnameNamePatronimic, 
            out string surname, out string name, out string patronimic)
        {
            if(surnameNamePatronimic == null) throw new ArgumentNullException("surnameNamePatronimic");

            surname = name = patronimic = null;

            FIOParts parts = new FIOParts();
            IntPtr ptr = IntPtr.Zero;
            try
            {
                parts = new FIOParts(m_MaxResultStringBufSize);
                ptr = StringToIntPtr(surnameNamePatronimic);

                int err = decGetFIOParts(ptr, ref parts);
                if(err < 0) err--;
                ThrowException(err);

                surname = IntPtrToString(parts.pSurname, parts.lenSurname);
                name = IntPtrToString(parts.pName, parts.lenName);
                patronimic = IntPtrToString(parts.pPatronimic, parts.lenPatronimic);
            }
            finally
            {
                parts.Free();
                Marshal.FreeHGlobal(ptr);
            }
        }

        #endregion Service functions

        #region Exceptions dictionary functions

        /// <summary>
        /// Значение, возвращаемое этой функцией, содержит информацию о том, что словарь исключений найден и 
        /// программа с ним работает. 
        /// </summary>
        /// <returns>true - если сларь найден, иначе - false.</returns>
        public static bool UpdateExceptionsDictionary()
        {
            return Convert.ToBoolean(decUpdateExceptions());
        }

        /// <summary>
        /// Устанавливает в качестве рабочего словарь fileName.
        /// Позволяет приложениям работать с различными словарями. Может быть полезной при разграничении прав 
        /// доступа пользователей. Осуществляет установку словаря в качестве рабочего и считывание из него 
        /// информации. 
        /// </summary>
        /// <param name="filename">Имя файла словаря</param>
        /// <returns>true - если словарь установлен и обновлен, иначе - false. </returns>
        public static bool SetExceptionsDictionaryFileName(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException("fileName");

            return Convert.ToBoolean(decSetDictionary(fileName));
        }

        /// <summary>
        /// Возвращает полное имя словаря исключений. Имя словаря исключений может потребоваться для модификации 
        /// словаря в процессе работы приложения, использующего функции DLL.
        /// </summary>
        /// <returns>Имя файла словаря</returns>
        public static string GetExceptionsDictionaryFileName()
        {
            StringBuilder sb = new StringBuilder(m_MaxResultStringBufSize);
            int tmp = m_MaxResultStringBufSize;
            int err = decGetExceptionsFileName(sb, ref tmp);
            ThrowException(err);
            return sb.ToString();
        }

        #endregion Exceptions dictionary functions

        #endregion Public properties and functions

        #region Private functions and fields

        private static Encoding encoding = Encoding.GetEncoding(1251);

        private static void CheckGender(Gender gender)
        {
            if(!Enum.IsDefined(typeof(Gender), gender) || gender == Gender.NotDefind)
                throw new ArgumentException("Недопустимое значение рода: " + gender, "gender");
        }

        private static void CheckDeclensionCase(DeclensionCase declensionCase)
        {
            if(!Enum.IsDefined(typeof(DeclensionCase), declensionCase) || 
                declensionCase == DeclensionCase.NotDefind)
                throw new ArgumentException("Недопустимое значение падежа: " + declensionCase, "declensionCase");
        }

        private static void FreeIntPtr(IntPtr[] ptrs)
        {
            if(ptrs != null) foreach(IntPtr ptr in ptrs) Marshal.FreeHGlobal(ptr);
        }

        private static string IntPtrToString(IntPtr ptr, int resultLen)
        {
            byte[] bRes = new byte[resultLen];
            Marshal.Copy(ptr, bRes, 0, resultLen);

            return encoding.GetString(bRes);
        }

        private static string IntPtrToString(IntPtr[] ptrs, int resultLen)
        {
            return IntPtrToString(ptrs[ptrs.Length - 1], resultLen);
        }

        private static IntPtr[] StringsToIntPtrArray(params string[] strs)
        {
            IntPtr[] ptrs = new IntPtr[strs.Length + 1];

            for(int i = 0; i < ptrs.Length - 1; i++) ptrs[i] = StringToIntPtr(strs[i]);

            ptrs[ptrs.Length - 1] = Marshal.AllocHGlobal(m_MaxResultStringBufSize);

            return ptrs;
        }

        private static IntPtr StringToIntPtr(string srcStr)
        {
            byte[] buf = new byte[srcStr.Length + 1];
            encoding.GetBytes(srcStr, 0, srcStr.Length, buf, 0);
            buf[srcStr.Length] = 0;

            IntPtr pBuf = Marshal.AllocHGlobal(buf.Length);
            Marshal.Copy(buf, 0, pBuf, buf.Length);

            return pBuf;
        }

        private static void ThrowException(int errorCode)
        {
            switch(errorCode)
            {
                case 0:
                    return;
                case -1:
                    throw new DeclensionException("Недопустимое значение падежа", errorCode);
                case -2:
                    throw new DeclensionException("Недопустимое значение рода", errorCode);
                case -3:
                    throw new DeclensionException("Размер буфера недостаточен для размещения результата преобразования ФИО",
                        errorCode);
                case -4:
                    throw new DeclensionException("Размер буфера surname(фамилия) недостаточен", errorCode);
                case -5:
                    throw new DeclensionException("Размер буфера name(имя) недостаточен", errorCode);
                case -6:
                    throw new DeclensionException("Размер буфера patronimic(отчество) недостаточен", errorCode);
            }
        }

        #endregion  Private functions and fields

        #region Padeg.dll functions and structs

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFIOPadeg")]
        private static extern Int32 decGetFIOPadeg(IntPtr surname, IntPtr name, IntPtr patronimic,
            Int32 sex, Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFIOPadegAS")]
        private static extern Int32 decGetFIOPadegAS(IntPtr surname, IntPtr name, IntPtr patronimic,
            Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFIOPadegFS")]
        private static extern Int32 decGetFIOPadegFS(IntPtr fio,
            Int32 sex, Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFIOPadegFSAS")]
        private static extern Int32 decGetFIOPadegFSAS(IntPtr fio,
            Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetIFPadeg")]
        private static extern Int32 decGetIFPadeg(IntPtr name, IntPtr surname,
            Int32 sex, Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetIFPadegFS")]
        private static extern Int32 decGetIFPadegFS(IntPtr nameSurname,
            Int32 sex, Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetNominativePadeg")]
        private static extern Int32 decGetNominativePadeg(IntPtr surnameNamePatronimic,
            IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetAppointmentPadeg")]
        private static extern Int32 decGetAppointmentPadeg(IntPtr appointment,
            Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFullAppointmentPadeg")]
        private static extern Int32 decGetFullAppointmentPadeg(IntPtr appointment, IntPtr office,
            Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetOfficePadeg")]
        private static extern Int32 decGetOfficePadeg(IntPtr office,
            Int32 padeg, IntPtr result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetSex")]
        private static extern Int32 decGetSex(IntPtr patronimic);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetPadegId")]
        private static extern Int32 decGetPadegId(IntPtr surnameNamePatronimic);

        [StructLayout(LayoutKind.Sequential)]
        private struct FIOParts
        {
            public IntPtr pSurname;
            public IntPtr pName;
            public IntPtr pPatronimic;
            public Int32 lenSurname;
            public Int32 lenName;
            public Int32 lenPatronimic;

            public FIOParts(int maxResultStringBufSize)
            {
                this.pSurname = Marshal.AllocHGlobal(maxResultStringBufSize);
                this.pName = Marshal.AllocHGlobal(maxResultStringBufSize);
                this.pPatronimic = Marshal.AllocHGlobal(maxResultStringBufSize);

                this.lenSurname = this.lenName = this.lenPatronimic = maxResultStringBufSize;
            }

            public void Free()
            {
                Marshal.FreeHGlobal(pPatronimic);
                Marshal.FreeHGlobal(pName);
                Marshal.FreeHGlobal(pSurname);
            }
        }

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetFIOParts")]
        private static extern Int32 decGetFIOParts(IntPtr surnameNamePatronimic, ref FIOParts result);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "UpdateExceptions")]
        private static extern Int32 decUpdateExceptions();

        [DllImport("Libs\\Padeg.dll", EntryPoint = "GetExceptionsFileName")]
        private static extern Int32 decGetExceptionsFileName([MarshalAs(UnmanagedType.LPTStr)]StringBuilder result, ref Int32 resultLength);

        [DllImport("Libs\\Padeg.dll", EntryPoint = "SetDictionary")]
        private static extern Int32 decSetDictionary([MarshalAs(UnmanagedType.LPTStr)]string fileName);

        #endregion Padeg.dll functions and structs
    }

    /// <summary>
    /// Класс для исключений библиотеки
    /// </summary>
    [Serializable]
    public class DeclensionException : Exception, ISerializable
    {
        private int m_ErrorCode;
        /// <summary>
        /// Код ошибки
        /// -1 - Недопустимое значение падежа
        /// -2 - Недопустимое значение рода
        /// -3 - Размер буфера недостаточен для размещения результата преобразования ФИО
        /// -4 - Размер буфера surname(фамилия) недостаточен
        /// -5 - Размер буфера name(имя) недостаточен"
        /// -6 - Размер буфера patronimic(отчество) недостаточен
        /// </summary>
        public int ErrorCode
        {
            get
            {
                return m_ErrorCode;
            }
            set
            {
                m_ErrorCode = value;
            }
        }

        public DeclensionException()
            : base()
        {
        }

        public DeclensionException(string message)
            : base(message)
        {
        }

        public DeclensionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DeclensionException(string message, int errorCode)
            : this(message)
        {
            m_ErrorCode = errorCode;
        }

        public DeclensionException(string message, int errorCode, Exception innerException)
            : this(message, innerException)
        {
            m_ErrorCode = errorCode;
        }

        private DeclensionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_ErrorCode = info.GetInt32("ErrorCode");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ErrorCode", m_ErrorCode);

            base.GetObjectData(info, context);
        }
    }
}
