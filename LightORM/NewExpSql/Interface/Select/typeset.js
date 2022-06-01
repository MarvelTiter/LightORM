let genericType =
    function (
        count,
    ) {
        let types = [];
        for (
            let index = 0; index <
            count; index++
        ) {
            types.push(
                `T${index +
                1
                }`,
            );
        }
        return types.join();
    };
let genericParam =
    function (
        count,
    ) {
        let types = [];
        for (
            let index = 0; index <
            count; index++
        ) {
            types.push(
                `T${index +
                1
                } t${index +
                1
                }`,
            );
        }
        return types.join();
    };
let genericInit =
    function (
        count,
    ) {
        let types = [];
        for (
            let index = 0; index <
            count; index++
        ) {
            types.push(
                `this.Tb${index +
                1
                } = t${index +
                1
                };`,
            );
        }
        return types.join("\n");
    };
let genericProp =
    function (
        count,
    ) {
        let types = [];
        for (
            let index = 0; index <
            count; index++
        ) {
            types.push(
                `public T${index +
                1
                } Tb${index +
                1
                } { get; }`,
            );
        }
        return types.join("\n");
    };

function Generate(
    count,
) {
    let template = ` public class TypeSet<${genericType(
        count,
    )}>
                                      {
                                          public TypeSet(${genericParam(
        count,
    )})
                                          {
                                             ${genericInit(
        count,
    )}
                                          }
                                          ${genericProp(
        count,
    )}
                                      }
                                  `;
    output.innerText = template
}