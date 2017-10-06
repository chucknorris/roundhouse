// ==============================================================================
// 
// Fervent Coder Copyright � 2011 - Released under the Apache 2.0 License
// 
// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
// ==============================================================================
namespace roundhouse.tests
{
    using Should;

    public class ExampleSpecs
    {
        public abstract class ExampleSpecsBase : TinySpec
        {
            protected int result;
            
            public override void Context() { }
        }

        [ConcernFor("Example")]
        public class when_adding_three_plus_three : ExampleSpecsBase
        {
            public override void Because()
            {
                result = 3 + 3;
            }

            [Fact]
            public void should_be_equal_to_six()
            {
                result.ShouldEqual(6);
            }
            
            [Fact]
            public void should_not_be_equal_to_nine()
            {
                result.ShouldNotEqual(9);
            }
        }

        [ConcernFor("Example")]
        public class when_subtracting_three_minus_one : ExampleSpecsBase
        {
            public override void Because()
            {
                result = 3 - 1;
            }

            [Fact]
            public void should_be_equal_to_two()
            {
                result.ShouldEqual(2);
            }
        }
    }
}