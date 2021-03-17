// <copyright file="jest.config.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

module.exports = {
    roots: ["<rootDir>/src"],
    globals: {
        "ts-jest": {
            tsConfig: "tsconfig.test.json"
        }
    },
    transform: {
        "^.+\\.tsx?$": "ts-jest"
    },
    moduleNameMapper: {
        "\\.(css|less|scss|sass)$": "identity-obj-proxy"
    },
    moduleDirectories: ['node_modules', 'src'],
    preset: 'ts-jest',
    testRegex: "(/__tests__/.*|(\\.|/)(test|spec))\\.tsx?$",
    moduleFileExtensions: ["ts", "tsx", "js", "jsx", "json", "node"],
    snapshotSerializers: ["enzyme-to-json/serializer"],
    setupTestFrameworkScriptFile: "<rootDir>/src/jest/setup.js"
};