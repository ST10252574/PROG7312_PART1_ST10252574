import { Router, type IRouter } from "express";
import healthRouter from "./health";
import smartxRouter from "./smartx";

const router: IRouter = Router();

router.use(healthRouter);
router.use(smartxRouter);

export default router;
