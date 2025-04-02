using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    // --- File: Session.cs ---

    // تعليق: مساحة الاسم يجب أن تكون متاحة للملفات الأخرى التي تحتاج إلى الوصول إلى الجلسة
    // Namespace should be accessible by other files needing session access
    namespace DataGridNamespace
    {
        /// <summary>
        /// تعليق: فئة ثابتة بسيطة لتخزين معلومات جلسة المستخدم الحالية.
        /// Simple static class to hold current user session information.
        /// </summary>
        public static class Session
        {
            // تعليق: خاصية ثابتة لتخزين معرف المستخدم المسجل دخوله حاليًا.
            // Static property to store the currently logged-in user's ID.
            // تعليق: القيمة الافتراضية -1 تشير إلى عدم تسجيل دخول أي مستخدم.
            // Default value -1 indicates no user is logged in.
            // تعليق: ملاحظة - في التطبيق الحقيقي، لا يجب تعيين قيمة افتراضية هنا إلا إذا كانت لأغراض الاختبار فقط.
            // NOTE: In a real app, don't default this unless purely for testing.
            public static int CurrentUserId { get; set; } = -1;

            // تعليق: يمكنك إضافة خصائص أخرى متعلقة بالجلسة هنا لاحقًا إذا لزم الأمر (مثل اسم المستخدم، الدور).
            // You can add other session-related properties here later if needed (like Username, Role).
            public static string CurrentUserRole { get; set; } = string.Empty; // Added Role as well
        }
    }




