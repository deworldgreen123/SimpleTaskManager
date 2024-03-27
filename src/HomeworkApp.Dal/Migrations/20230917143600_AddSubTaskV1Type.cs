using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;

[Migration(20230917143600, TransactionBehavior.None)]
public class AddSubTaskV1Type: Migration {
    public override void Up()
    {
        const string sql = @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'sub_tasks_v1') THEN
            CREATE TYPE sub_tasks_v1 as
            (
                  id                  bigint
                , title               text
                , status              integer
                , parents_ids         bigint[]
            );
        END IF;
    END
$$;";
        
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO $$
    BEGIN
        DROP TYPE IF EXISTS sub_tasks_v1;
    END
$$;";

        Execute.Sql(sql);
    }
}