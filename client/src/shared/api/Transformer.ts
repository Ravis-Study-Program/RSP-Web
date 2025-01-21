import { OpenAPIObject } from 'openapi3-ts/oas30';

export const customTransformer = (inputSchema: OpenAPIObject): OpenAPIObject => {
  if (inputSchema.components?.schemas) {
    Object.values(inputSchema.components.schemas).forEach((schema: any) => {
      if (schema.properties) {
        const updatedProperties: Record<string, any> = {};
        Object.entries(schema.properties).forEach(([propKey, propValue]) => {
          // Convert the first character of the property name to lowercase (simple camelCase)
          const camelCaseKey = propKey.charAt(0).toLowerCase() + propKey.slice(1);
          updatedProperties[camelCaseKey] = propValue;
        });
        schema.properties = updatedProperties;
      }
    });
  }
  return inputSchema;
};
