namespace HVM.Experiments;

public static class Constants
{
    public const string Faulty = "this is faulty";
    public const string Stress = 
      """
      @fun = (?((@fun__C0 @fun__C1) a) a)
      
      @fun__C0 = a
        & @loop ~ (65536 a)
      
      @fun__C1 = ({a b} d)
        &! @fun ~ (a $([+] $(c d)))
        &! @fun ~ (b c)
      
      @loop = (?((0 @loop__C0) a) a)
      
      @loop__C0 = a
        & @loop ~ a
      
      @main = a
        & @fun ~ (8 a)
      
      """;
    public const string BitonicSort = 
        """
        @down = (?(((a (* a)) @down__C0) (b (c d))) (c (b d)))
        
        @down__C0 = ({a e} ((c g) ({b f} (d h))))
          &!@flow ~ (a (b (c d)))
          &!@flow ~ (e (f (g h)))
        
        @flow = (?(((a (* a)) @flow__C0) (b (c d))) (c (b d)))
        
        @flow__C0 = ({$([+1] a) c} ((e f) ({b d} h)))
          & @down ~ (a (b (g h)))
          & @warp ~ (c (d (e (f g))))
        
        @gen = (a b)
          & @gen__bend0 ~ (a (0 b))
        
        @gen__bend0 = ({?(((* (a a)) @gen__bend0__C0) (b c)) b} c)
        
        @gen__bend0__C0 = (* ({$([:-1] a) $([:-1] d)} ({$([*2] $([+1] b)) $([*2] e)} (c f))))
          &!@gen__bend0 ~ (a (b c))
          &!@gen__bend0 ~ (d (e f))
        
        @main = a
          & @sum ~ (18 (@main__C1 a))
        
        @main__C0 = a
          & @gen ~ (18 a)
        
        @main__C1 = a
          & @sort ~ (18 (0 (@main__C0 a)))
        
        @sort = (?(((a (* a)) @sort__C0) (b (c d))) (c (b d)))
        
        @sort__C0 = ({$([+1] a) {c f}} ((d g) (b i)))
          & @flow ~ (a (b ((e h) i)))
          &!@sort ~ (c (0 (d e)))
          &!@sort ~ (f (1 (g h)))
        
        @sum = (?(((a a) @sum__C0) b) b)
        
        @sum__C0 = ({a c} ((b d) f))
          &!@sum ~ (a (b $([+] $(e f))))
          &!@sum ~ (c (d e))
        
        @swap = (?((@swap__C0 @swap__C1) (a (b c))) (b (a c)))
        
        @swap__C0 = (b (a (a b)))
        
        @swap__C1 = (* (a (b (a b))))
        
        @warp = (?((@warp__C0 @warp__C1) (a (b (c d)))) (c (b (a d))))
        
        @warp__C0 = ({a e} ({$([>] $(a b)) d} ($([+] $(b c)) f)))
          & @swap ~ (c (d (e f)))
        
        @warp__C1 = ({a f} ((d i) ((c h) ({b g} ((e j) (k l))))))
          &!@warp ~ (f (g (h (i (j l)))))
          &!@warp ~ (a (b (c (d (e k)))))
        
        """;
    public const string Fib = 
      """
      @fib_iterative = a
        & @fib_iterative__bend0 ~ (0 (1 a))
      
      @fib_iterative__bend0 = (b (c ({$([!0] ?(((a (* (* a))) @fib_iterative__bend0__C0) (b (c (d e))))) d} e)))
      
      @fib_iterative__bend0__C0 = (* ($([+] $(b c)) ({a b} ($([:-1] d) e))))
        & @fib_iterative__bend0 ~ (a (c (d e)))
      
      @fib_recursive = (?((0 @fib_recursive__C1) a) a)
      
      @fib_recursive__C0 = ({a $([+1] b)} d)
        &!@fib_recursive ~ (a $([+] $(c d)))
        &!@fib_recursive ~ (b c)
      
      @fib_recursive__C1 = (?((1 @fib_recursive__C0) a) a)
      
      @main = a
        & @fib_iterative ~ (30 a)
      
      """;
}