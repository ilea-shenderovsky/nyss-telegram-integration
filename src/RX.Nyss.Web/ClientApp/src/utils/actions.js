export const action = (name) => ({
    INVOKE: `${name}_INVOKE`,
    REQUEST: `${name}_REQUEST`,
    SUCCESS: `${name}_SUCCESS`,
    FAILURE: `${name}_FAILURE`,
    getId: (id) => `${name}_${id}`,
    invoke: (args) => ({ type: `${name}_INVOKE`, ...args }),
    request: (args) => ({ type: `${name}_REQUEST`, ...args }),
    success: (args) => ({ type: `${name}_SUCCESS`, ...args }),
    failure: (args) => ({ type: `${name}_FAILURE`, ...args }),
});