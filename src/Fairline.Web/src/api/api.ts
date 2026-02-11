import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type {
  ApiStatusResponse,
  CatalogRefreshResult,
  CatalogResponse,
  DashboardResponse,
  IngestRunDetail,
  IngestRunSummary,
  ProviderInfo,
  RunIngestionRequest,
  ScenarioSummary,
} from "../types";

export const api = createApi({
  baseQuery: fetchBaseQuery({ baseUrl: "" }),
  tagTypes: ["Catalog", "Runs"],
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

    // Ingestion endpoints
    getCatalog: builder.query<CatalogResponse, void>({
      query: () => "/api/ingest/catalog",
      providesTags: ["Catalog"],
    }),
    refreshCatalog: builder.mutation<CatalogRefreshResult, void>({
      query: () => ({ url: "/api/ingest/catalog/refresh", method: "POST" }),
      invalidatesTags: ["Catalog"],
    }),
    toggleTrackedLeague: builder.mutation<
      void,
      { providerSportKey: string; enabled: boolean }
    >({
      query: (body) => ({
        url: "/api/ingest/catalog/track",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Catalog"],
    }),
    runIngestion: builder.mutation<{ runId: string }, RunIngestionRequest>({
      query: (body) => ({ url: "/api/ingest/run", method: "POST", body }),
    }),
    getRuns: builder.query<IngestRunSummary[], number | void>({
      query: (limit) => `/api/ingest/runs${limit ? `?limit=${limit}` : ""}`,
      providesTags: ["Runs"],
    }),
    getRunDetail: builder.query<IngestRunDetail, string>({
      query: (runId) => `/api/ingest/runs/${runId}`,
    }),

    // Dashboard
    getDashboard: builder.query<DashboardResponse, void>({
      query: () => "/api/dashboard",
    }),
  }),
});

export const {
  useGetStatusQuery,
  useGetProvidersQuery,
  useGetScenariosQuery,
  useGetCatalogQuery,
  useRefreshCatalogMutation,
  useToggleTrackedLeagueMutation,
  useRunIngestionMutation,
  useGetRunsQuery,
  useGetRunDetailQuery,
  useGetDashboardQuery,
} = api;
