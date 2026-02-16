<style>
    .tc-table {
        width: 100%;
        border-collapse: collapse;
        border: 1px solid #e5e7eb;
        border-radius: 10px;
        overflow: hidden;
    }

    .tc-table th,
    .tc-table td {
        padding: 10px 10px;
        border-bottom: 1px solid #e5e7eb;
        vertical-align: top;
    }

    .tc-table thead th {
        background: #f8fafc;
        color: #6b7280;
        font-weight: 600;
        font-size: 12px;
        text-transform: uppercase;
        letter-spacing: 0.06em;
    }

    .tc-table tbody td {
        font-size: 13px;
    }

    .tc-table .num {
        text-align: right;
        white-space: nowrap;
    }

    .tc-table .muted {
        color: #6b7280;
        font-size: 12.5px;
    }
</style>

<table class="tc-table" role="table" aria-label="Invoice items">
    <thead>
        <tr>
            <th style="width: 14%;">[Col_ItemCode]</th>
            <th>[Col_ItemDescription]</th>
            <th style="width: 18%;">[Col_ItemReference]</th>
            <th style="width: 10%;">[Col_TaxCode]</th>
            <th class="num" style="width: 14%;">[Col_InvoiceValue]</th>
            <th class="num" style="width: 14%;">[Col_TaxValue]</th>
            <th class="num" style="width: 14%;">[Col_TotalValue]</th>
        </tr>
    </thead>

    <tbody>
        <!--ITEM-->
        <tr>
            <td><div><strong>[ItemCode]</strong></div></td>
            <td>
                <div>[ItemDescription]</div>
                <div class="muted">[ItemReference]</div>
            </td>
            <td>[ItemReference]</td>
            <td>[TaxCode]</td>
            <td class="num">[InvoiceValue]</td>
            <td class="num">[TaxValue]</td>
            <td class="num">[TotalValue]</td>
        </tr>
        <!--/ITEM-->

        [Items]
    </tbody>
</table>
