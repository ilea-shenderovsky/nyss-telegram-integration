import { placeholders } from "../../../siteMapPlaceholders";
import { accessMap } from "../../../authentication/accessMap";
import { strings, stringKeys } from "../../../strings";
import { projectLeftMenuOrder } from "../../projects/logic/projectsSiteMap";

export const alertsSiteMap = [
  {
    parentPath: "/nationalsocieties/:nationalSocietyId/projects/:projectId",
    path: "/projects/:projectId/alerts",
    title: () => strings(stringKeys.alerts.title),
    placeholder: placeholders.leftMenu,
    access: accessMap.alerts.list,
    placeholderIndex: projectLeftMenuOrder.alerts
  },
  {
    parentPath: "/projects/:projectId/alerts",
    path: "/projects/:projectId/alerts/:alertId/details",
    title: () => strings(stringKeys.alerts.details.title),
    access: accessMap.alerts.assess
  },
  {
    parentPath: "/projects/:projectId/alerts/:alertId/details",
    path: "/projects/:projectId/alerts/:alertId/assess",
    title: () => strings(stringKeys.alerts.assess.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.alerts.assess,
    middleStepOnly: true
  },
  {
    parentPath: "/projects/:projectId/alerts/:alertId/details",
    path: "/projects/:projectId/alerts/:alertId/logs",
    title: () => strings(stringKeys.alerts.logs.title),
    placeholder: placeholders.tabMenu,
    access: accessMap.alerts.assess,
    middleStepOnly: true
  }
];
