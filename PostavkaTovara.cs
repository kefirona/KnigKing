//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KnigKing
{
    using System;
    using System.Collections.Generic;
    
    public partial class PostavkaTovara
    {
        public int ID { get; set; }
        public int ID_Tovar { get; set; }
        public int KolVo { get; set; }
    
        public virtual Tovar Tovar { get; set; }
    }
}
