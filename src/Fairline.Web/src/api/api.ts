import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { ApiStatusResponse, ProviderInfo, ScenarioSummary } from "../types";

export const api = createApi({
  baseQuery: fetchBaseQuery({ baseUrl: "" }),
  endpoints: (builder) => ({
    getStatus: builder.query<ApiStatusResponse, void>({
      query: () => "/api/status",
    }),
    getProviders: builder.query<ProviderInfo[], void>({
      query: () => "/api/ingest/providers",
    }),
    getScenarios: builder.query<ScenarioSummary[], void>({
      query: () => "/api/modeling/scenarios",
    }),
  }),
});

export const { useGetStatusQuery, useGetProvidersQuery, useGetScenariosQuery } =
  api;
